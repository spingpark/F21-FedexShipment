--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--<F21SP>**************************************************************
--*        SP Name        :  spGetFedExScanCarton
--*		   Ref SP		  :  EXEC spKR_UPSIntegration;130 '0001','100506500004', @NewID OUT
--*        Exe example    :  
--*        Created  By    :  Jinbeom.p
--*        Created Date   :  05/17/2018
--*        Used by        :  FedExShipments >> Print Lable >> Carton Label
--*        Description    :  Old Carton 입력시 NewCarton이 있는 경우 NewCartonID 리턴.(없는 경우 Old Carton리턴)
-----------------------------------------------------------------------
-- No :  Date Modified	: Developer		: Description 
-- 01 : 05/21/2018		: Yangsub Lee	: Add FedEx, Ontrac & Carton Length check
-- 02 : 03/08/2019		: Jinbeom.p		: MCID Check 후 해당 안될 시 빈값 리턴 (잘못 스캔된 MCID 및 19자리 예외처리)
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetFedExScanCarton
	@DataOwnerID	VARCHAR(4) = '0000',
	@CartonID		VARCHAR(50),
	@NewCartonID	VARCHAR(19) OUTPUT
AS
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED


-- SCM21 Sub-5 릴리즈시 활성화
-- 02 : 03/08/2019		: Jinbeom.p		: MCID Check 후 해당 안될 시 빈값 리턴
SET @NewCartonID =''

-- MCID Barcode인 경우
IF LEN(@CartonID) = 19 AND NOT EXISTS(SELECT 1 FROM tblStoreMapping WHERE StoreID = SUBSTRING(@CartonID,3,4) AND SubStoreID = SUBSTRING(@CartonID,7,4))
BEGIN
	SET @NewCartonID = (SELECT	TOP 1 CartonID 
						FROM (	SELECT	CartonID
								FROM	tblVendorPackingListMaster 
								WHERE	MCID = @CartonID 
								UNION 
								SELECT	CartonID
								FROM	tblVendorPackingListMaster 
								WHERE	MCID IN (	SELECT	LogicalMCID 
															FROM	dbo.tblMappingMCID 
															WHERE	PhysicalMCID = @CartonID) ) a ) 
	
	-- 02 : 03/08/2019		: Jinbeom.p		: MCID Check 후 해당 안될 시 빈값 리턴
	IF LEN(@NewCartonID) = 0
		SET @NewCartonID = ''
		RETURN
END	


-- FedEx Open 시점에는 tblCartonAdjustmentAfterPost를 NW에서 진행하기 때문에 추후 변경
-- Carton Adjsutment After Post
IF LEN(@NewCartonID) = 0
	SET @NewCartonID = (SELECT TOP 1 NextCartonID
						FROM tblCartonAdjustmentAfterPostHistory 
						WHERE PrevCartonID = @CartonID ORDER BY CAAPHAutoID DESC)

-- Carton Master check
IF LEN(@NewCartonID) = 0
	SET @NewCartonID = (SELECT NextCartonID FROM tblCartonMaster WHERE CartonID = @CartonID)


-- FedEx Barcode check
IF LEN(@CartonID) = 34 
	IF EXISTS(SELECT TOP 1 OrderNumber FROM tblFedExDayEnd WHERE Barcode = @CartonID)
		SET @NewCartonID = (SELECT TOP 1 OrderNumber FROM tblFedExDayEnd WHERE Barcode = @CartonID)

-- Ontrac TrackingNumber check
IF LEN(@CartonID) = 15 AND LEFT(@CartonID,1) = 'D'
	IF EXISTS(SELECT TOP 1 OrderNumber FROM tblOnTracDayEnd WHERE OnTracTrackingNumber = @CartonID)
		SET @NewCartonID = (SELECT TOP 1 OrderNumber FROM tblOnTracDayEnd WHERE OnTracTrackingNumber = @CartonID)

IF ISNULL(@NewCartonID,'') = '' AND LEN(@CartonID) IN(12, 19)
	SET @NewCartonID = @CartonID;		-- 치환할 Carton이 없거나 12, 19자리 Carton이면 정상


---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--<F21SP>***************************************************************
--* SP Name			: spFedExShipments
--* Ref example		: EXEC spFedExShipments;2 '168501112345'
--* Created  By		: Jinbeom.p
--* Created Date	: 05/17/2018
--* Used by			: FedEx shipments Weight Search
--* Description		: CartonID Weight 조회
-----------------------------------------------------------------------
-- No : Date Modified	: Developer     : Description
-- 01 : 05/21/2018		: Yangsub Lee	: 일반 Carton인지 체크 로직 추가
-- 02 : 04/18/2018		: Jinbeom p		: Default Weight 16
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetFedExCartonWeight
	@CartonID			VARCHAR(19)
AS
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

DECLARE @CartonWeight		NUMERIC(8,2)
SET @CartonWeight = 16.00

IF LEN(@CartonID) = 19
BEGIN
	-- Store, SubStore check
	IF NOT EXISTS(SELECT TOP 1 1 FROM tblStoreMapping WHERE StoreID = SUBSTRING(@CartonID,3,4) AND SubStoreID = SUBSTRING(@CartonID,7,4))
		SET @CartonWeight = ISNULL((SELECT TOP 1 DimensionWeight AS CartonWeight
							FROM SCMVendorExtranet.dbo.tblVendorPackingList WITH(NOLOCK)
							WHERE OrderNumber = LEFT(@CartonID,8) 
								AND MCIDStart <= @CartonID AND MCIDEnd >= @CartonID), 15)	
END
-- Weight Return;
SELECT @CartonWeight CartonWeight


--<F21SP>***************************************************************
--* SP Name			: spFedExShipments
--* Ref example		: EXEC spFedExShipments;1 '700145008184',26.5,'881280'
--* Created  By		: Jinbeom.p
--* Created Date	: 05/17/2018
--* Used by			: FedEx shipments Search
--* Description		: CartonID ??
-----------------------------------------------------------------------
-- No : Date Modified	: Developer     : Description
-- 01 : 05/21/2018		: Yangsub Lee	: 
-- 02 : 05/24/2018		: Jinbeom.p		: BannerName SubStoreID ??  , RegEmpiD ?? ?? CreateUserID? ??, ReceiveSubName ??
-- 03 : 07/05/2018		: Jinbeom.p		: ZPL Code128 / 39 Swap 
-- 04 : 07/24/2018		: Yangsub Lee	: Change Length @UserID 10 > 30
-- 05 : 08/22/2018		: Yangsub Lee	: Add ScanBarcode Log 
-- 06 : 8/30/2018		: HyeJin Kim	: Add ScanBarcode Log for missin new carton ID
-- 07 : 02/28/2018		: Jinbeom.p		: [Manual Fedex program - Weight issue] Close Fedex Reprint Result -> 0
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetFedExCartonInfo
	@CartonID			VARCHAR(50),
	@Weight				Numeric(9,2),
	@UserID				VARCHAR(30),
	@Type				VARCHAR(1) = '0',		-- 0 : FedEx Shipments, 1 : Panda Manual
	@BarcodeSizeType	VARCHAR(1) = '0',		-- 0 : BY3, 1 : BY4
	@BarcodeKind		VARCHAR(1) = 'Y'        -- 'Y' : Code128, 'N' : Code39   -- 03 : 07/05/2018		: Jinbeom.p		: ZPL Code128 / 39 Swap 
AS
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

DECLARE @NewCartonID		VARCHAR(19)
DECLARE @ScanBarcode		VARCHAR(19)
DECLARE	@Store				VARCHAR(4),
		@SubStore			VARCHAR(4),
		@OldStore			VARCHAR(4),
		@NewType			VARCHAR(4),
		@TransportCodeName	VARCHAR(100),
		@Service			VARCHAR(4),
		@Address			VARCHAR(100),
		@Address2			VARCHAR(100),
		@ConsigneeCity		VARCHAR(100),
		@State				VARCHAR(10),
		@Zip1				VARCHAR(10),
		@Zip2				VARCHAR(10),
		@TruckNumber		VARCHAR(100),
		@TelNo				VARCHAR(100),
		@BannerName			VARCHAR(100)
		
  
--New CartonID ??
EXEC spGetFedExScanCarton '0000',@CartonID, @NewCartonID OUTPUT

-- Add ScanBarcode Log,08/22/2018 
SET @ScanBarcode = @CartonID

IF LEN(@NewCartonID) != 0
	BEGIN
		SET @CartonID = @NewCartonID
	END
ELSE
	BEGIN
		-- 06 : 8/30/2018		: HyeJin Kim	: Add ScanBarcode Log for missin new carton ID
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, '', '','','','','','','','', '', '', '', '', '0', '', '',@CartonID, '', '', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
		RETURN;		-- NewCartonID? ?? ?? ? ?? CartonID? ???? Return ??
	END

-- Carton Length 12, 19??? ?? ??
IF LEN(@NewCartonID)= 12
	SELECT	@Store = StoreID, @SubStore = SubStoreID, @OldStore = OldStoreID
	FROM	tblStoreMapping	
	WHERE	OldStoreID = SUBSTRING(@CartonID,3,4)
ELSE
	SELECT	@Store = StoreID, @SubStore = SubStoreID, @OldStore = OldStoreID
	FROM	tblStoreMapping	
	WHERE	StoreID = SUBSTRING(@CartonID,3,4) AND SubStoreID = SUBSTRING(@CartonID,7,4)

-- Get Store Infomation
SELECT	@NewType = ISNULL(a.BrandID,''),
		@TransportCodeName = ISNULL((SELECT c.Code+rtrim(c.CodeName) 
									FROM dbo.tblCodeDetail c 
									WHERE c.Class='0008' AND c.Code=ad.Transport),''),
		@Service = ISNULL(ad.Service,''), 
		@Address = ISNULL(ad.Address,''), 
		@Address2 = ISNULL(ad.Address2,''), 
		@ConsigneeCity = ISNULL(ad.City,''), 
		@State = ISNULL(ad.StateCode,''), 
		@Zip1 = ISNULL(ad.Zip1,''), 
		@Zip2 = ISNULL(ad.Zip2,''),
		@TruckNumber = ISNULL((SELECT c.CodeFullName FROM dbo.tblCodeDetail c WHERE c.Class='0230' AND c.Code=t.TruckCode),''),
		@TelNo = ISNULL(TelNo,'')
FROM	dbo.tblStore a
		INNER JOIN dbo.tblStoreAddInfo ad ON(ad.StoreID = a.StoreID)
		LEFT OUTER JOIN
		dbo.tblStoreTruckNumber t on(a.StoreID=t.StoreID)
WHERE	a.StoreID = @Store 

-- Fedex?? ??? ??.
IF LEFT(@TransportCodeName,4) IN('0005','0066','0067','0068', '0086')		-- UPS? FedEx?? ??
BEGIN	
	SET @BannerName = (SELECT dbo.fnGetCodeFullName('0805',BannerID) BannerName FROM dbo.tblStoreAddInfo WHERE StoreID=@Store)

	IF EXISTS (SELECT TOP 1 OrderNumber 
			   FROM tblFedexDayEnd
			   WHERE OrderNumber = @CartonID
				AND OpenShippingCloseYN = 'N' 
				AND (CreateUserID='FedEx_PandaPP' OR CreateUserID='FedEx_Scheduler' OR CreateUserID = 'FedEx_PandaBeumer'
				AND CreateDate >= '2016-10-06')) 
	  AND @UserID NOT IN ('FedEx_PandaPP','FedEx_Scheduler', 'FedEx_PandaBeumer')
	BEGIN
		-- Open Shipments?? ??? Data? ???? ?? ??
		DELETE 
		FROM tblFedexDayEnd
		WHERE OrderNumber = @CartonID

		SELECT	ReceiveName		= @Store,
				ReceiveSubName  = @SubStore,
				Organization	= '',
				city			= @ConsigneeCity,
				line			= @Address,
				line2			= '',
				state			= @State,
				countryCode		= 'US',
				countryName		= 'United States',
				postalCode		= @Zip1,
				tel				= @TelNo,
				mobile			= '',
				email			= 'Store' + @Store + '@Forever21.com',
				ResultStatus	= '0',
				ResultZPL		= CONVERT(VARCHAR(4000),''),
				FedexTrackingNo	= '',			
				NewCartonID		= @CartonID,
				-- 02 : 05/24/2018		: Jinbeom.p		: BannerName SubStoreID ??			
				BannerName		= @Store + ':' + @SubStore + '  ' + @BannerName,
				ShippingCompany = 'Fedex'

		-- Add by yangsub Lee on 08/22/2018, Add Log
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,'',@State,
			'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', '0', '', '',
			@CartonID, @Store + ':' + @SubStore + '  ' + @BannerName, 'Fedex', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
	END
	ELSE 
	IF EXISTS (SELECT TOP 1 OrderNumber 
			   FROM tblFedexDayEnd
			   WHERE OrderNumber=@CartonID)
	BEGIN
		DECLARE @ResultStatus			VARCHAR(1),
				@IsSuccess				VARCHAR(1),
				@LabelString			VARCHAR(4000),
				@FedexTrackingNumber	VARCHAR(50),
				@OpenShippingCloseYN	VARCHAR(1)
		-- 07 : 02/28/2018		: Jinbeom.p		: [Manual Fedex program - Weight issue] Close Fedex Reprint Result -> 0
		SELECT	TOP 1 
				@IsSuccess = ISNULL(IsSuccess,''),
				@LabelString = ISNULL(LabelString,''),
				@FedexTrackingNumber = ISNULL(FedexTrackingNumber,''),
				@OpenShippingCloseYN = ISNULL(OpenShippingCloseYN,'')
		FROM	tblFedexDayEnd
		WHERE	OrderNumber=@CartonID
		ORDER BY AutoId DESC
		
		IF((@IsSuccess = 'Y') AND (@LabelString <> ''))
			
			IF @OpenShippingCloseYN = 'Y' AND @Type = '0'		-- FedExShipments에서 OpenShipment가 close된 경우 삭제하고 다시 출력
			BEGIN
				INSERT INTO dbo.tblFedexDayEndLog
						( AutoId ,OrderNumber , IsLive , HighestSeverity , FedexTrackingNumber ,  ShippingMethodName , ServiceType , PackageWeight , ImageType ,ReceiveName ,
						  ReceiveOrganization ,ReceiveLine1 , ReceiveLine2 , ReceiveCity ,ReceiveState ,ReceiveCountryCode , ReceiveCountryName , ReceivePostalCode ,
						  ReceiveTel ,FormId , Barcode , BarcodeType , UrsaPrefixCode , UrsaSuffixCode ,DestinationLocationId , AirportId , TransitTime , PickupCode ,
						  Machinable ,UspsApplicationId ,IsSuccess , NoticeCode , NoticeMessage , NoticeSeverity , NoticeSource , ErrorMessage ,FilePath , LabelString ,
						  PrintYN , PrintDate , Attribute1 , Attribute2 , Attribute3 , RegDate , RegEmpId ,RePrintYN ,RePrintDate ,CloseDate ,  CloseEmpId , OpenShippingCloseYN ,
						  OpenShippingCloseDate , CreateUserID ,  CreateDate ,UpdateUserID , UpdateDate)			
				SELECT TOP 1  AutoId ,OrderNumber , IsLive , HighestSeverity , FedexTrackingNumber ,  ShippingMethodName , ServiceType , PackageWeight , ImageType ,ReceiveName ,
						  ReceiveOrganization ,ReceiveLine1 , ReceiveLine2 , ReceiveCity ,ReceiveState ,ReceiveCountryCode , ReceiveCountryName , ReceivePostalCode ,
						  ReceiveTel ,FormId , Barcode , BarcodeType , UrsaPrefixCode , UrsaSuffixCode ,DestinationLocationId , AirportId , TransitTime , PickupCode ,
						  Machinable ,UspsApplicationId ,IsSuccess , NoticeCode , NoticeMessage , NoticeSeverity , NoticeSource , ErrorMessage ,FilePath , LabelString ,
						  PrintYN , PrintDate , Attribute1 , Attribute2 , Attribute3 , RegDate , RegEmpId ,RePrintYN ,RePrintDate ,CloseDate ,  CloseEmpId , OpenShippingCloseYN ,
						  OpenShippingCloseDate , CreateUserID ,  CreateDate ,UpdateUserID , UpdateDate FROM dbo.tblFedexDayEnd WHERE OrderNumber =@CartonID
			
				DELETE 
				FROM tblFedexDayEnd
				WHERE OrderNumber = @CartonID

				SET @ResultStatus = '0'
			END
			ELSE
			BEGIN
				SET @ResultStatus = '2' --Reprint
			END
		ELSE
			SET @ResultStatus = '3' --Error
		

		SELECT	ReceiveName		= @Store,	
			ReceiveSubName  = @SubStore,
			Organization	= '',
			city			= @ConsigneeCity,
			line			= @Address,
			line2			= '',
			state			= @State,
			countryCode		= 'US',
			countryName		= 'United States',
			postalCode		= @Zip1,
			tel				= @TelNo,
			mobile			= '',
			email			= 'Store' + @Store + '@Forever21.com',
			ResultStatus	= @ResultStatus,
			ResultZPL		= IIF(@ResultStatus='0', CONVERT(VARCHAR(4000),''), @LabelString),
			FedexTrackingNo = IIF(@ResultStatus='0', '', @FedexTrackingNumber),		
			NewCartonID		= @CartonID,	
			-- 02 : 05/24/2018		: Jinbeom.p		: BannerName SubStoreID ??			
			BannerName		= @Store + ':' + @SubStore + '  ' + @BannerName,
			ShippingCompany = 'Fedex'

		-- Add by yangsub Lee on 08/22/2018, Add Log
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,'',@State,
			'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', @ResultStatus, IIF(@ResultStatus='0','',@LabelString), IIF(@ResultStatus='0','',@FedexTrackingNumber),
			@CartonID, @Store + ':' + @SubStore + '  ' + @BannerName, 'Fedex', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)

	END
	ELSE
	BEGIN

		SELECT	ReceiveName		= @Store,
				ReceiveSubName  = @SubStore,
				Organization	= '',
				city			= @ConsigneeCity,
				line			= @Address,
				line2			= '',
				state			= @State,
				countryCode		= 'US',
				countryName		= 'United States',
				postalCode		= @Zip1,
				tel				= @TelNo,
				mobile			= '',
				email			= 'Store' + @Store + '@Forever21.com',
				ResultStatus	= '0',
				ResultZPL		= CONVERT(VARCHAR(4000),''),
				FedexTrackingNo	= '',				
				NewCartonID		= @CartonID,	
				-- 02 : 05/24/2018		: Jinbeom.p		: BannerName SubStoreID ??		
				BannerName		= @Store + ':' + @SubStore + '  ' + @BannerName,
				ShippingCompany = 'Fedex'

		-- Add by yangsub Lee on 08/22/2018, Add Log
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,'',@State,
			'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', '0', '', '',
			@CartonID, @Store + ':' + @SubStore + '  ' + @BannerName, 'Fedex', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
	END	
END
ELSE IF LEFT(@TransportCodeName,4) in ('0097')			-- OnTrac Data
BEGIN
	--Added by Yangsub Lee on 10/27/2016, Add BannerName
	SET @BannerName = (SELECT dbo.fnGetCodeFullName('0805',BannerID) BannerName FROM dbo.tblStoreAddInfo WHERE StoreID=@Store)

	IF EXISTS (SELECT TOP 1 OrderNumber 
			 FROM tblOnTracDayEnd
			   WHERE OrderNumber = @CartonID				
				AND PrintYN = 'N'
				AND (CreateUserID='FedEx_PandaPP' OR CreateUserID='FedEx_Scheduler' OR CreateUserID='FedEx_PandaBeumer' 
				AND CreateDate >= '2016-10-06')) 
	  AND @UserID NOT IN ('FedEx_PandaPP','FedEx_Scheduler','FedEx_PandaBeumer')
	BEGIN
		-- Open Shipments?? ??? Data? ???? ?? ??
		DELETE 
		FROM tblOnTracDayEnd
		WHERE OrderNumber=@CartonID

		SELECT	ReceiveName		= @Store,
				ReceiveSubName  = @SubStore,
				Organization	= '',
				city			= @ConsigneeCity,
				line			= @Address,
				line2			= @Address2,
				state			= @State,
				countryCode		= 'US',
				countryName		= 'United States',
				postalCode		= @Zip1,
				tel				= @TelNo,
				mobile			= '',
				email			= 'Store' + @Store + '@Forever21.com',
				ResultStatus	= '0',
				ResultZPL		= CONVERT(VARCHAR(4000),''),
				FedexTrackingNo	= '',				
				NewCartonID		= @CartonID,			
				BannerName		= @BannerName + '  ' + @Store + ':' + @SubStore,
				ShippingCompany = 'OnTrac'

		-- Add by yangsub Lee on 08/22/2018, Add Log
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,@Address2,@State,
			'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', '0', '', '',
			@CartonID, @BannerName + '  ' + @Store+ ':' + @SubStore, 'OnTrac', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
	END
	ELSE 
	IF EXISTS (SELECT TOP 1 OrderNumber 
			   FROM tblOnTracDayEnd
			   WHERE OrderNumber = @CartonID)
	BEGIN
		DECLARE @OnTracTrackingNumber	VARCHAR(50)
		
		SELECT	TOP 1 
				@IsSuccess = ISNULL(IsSuccess,''),
				@LabelString = ISNULL(LabelString,''),
				@OnTracTrackingNumber = ISNULL(OnTracTrackingNumber,'')
		FROM	tblOnTracDayEnd
		WHERE	OrderNumber=@CartonID
		ORDER BY AutoId DESC
		
		IF((@IsSuccess = 'Y') AND (@LabelString <> ''))
			SET @ResultStatus = '2' --Reprint
		ELSE
			SET @ResultStatus = '3' --Error
		

		SELECT	ReceiveName		= @Store,
				ReceiveSubName  = @SubStore,
				Organization	= '',
				city			= @ConsigneeCity,
				line			= @Address,
				line2			= @Address2,
				state			= @State,
				countryCode		= 'US',
				countryName		= 'United States',
				postalCode		= @Zip1,
				tel				= @TelNo,
				mobile			= '',
				email			= 'Store' + @Store + '@Forever21.com',
				ResultStatus	= @ResultStatus,
				ResultZPL		= @LabelString,
				FedexTrackingNo = @OnTracTrackingNumber,			
				NewCartonID		= @CartonID,				
				BannerName		= @BannerName + '  ' + @Store+ ':' + @SubStore,
				ShippingCompany = 'OnTrac'

		-- Add by yangsub Lee on 08/22/2018, Add Log
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,@Address2,@State,
			'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', @ResultStatus, @LabelString, @OnTracTrackingNumber,
			@CartonID, @BannerName + '  ' + @Store+ ':' + @SubStore, 'OnTrac', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
	END
	ELSE
	BEGIN
		SELECT	ReceiveName		= @Store,
				ReceiveSubName  = @SubStore,
				Organization	= '',
				city			= @ConsigneeCity,
				line			= @Address,
				line2			= @Address2,
				state			= @State,
				countryCode		= 'US',
				countryName		= 'United States',
				postalCode		= @Zip1,
				tel				= @TelNo,
				mobile			= '',
				email			= 'Store' + @Store + '@Forever21.com',
				ResultStatus	= '0',
				ResultZPL		= CONVERT(VARCHAR(4000),''),
				FedexTrackingNo	= '',				
				NewCartonID		= @CartonID,				
				BannerName		= @BannerName + '  ' + @Store+ ':' + @SubStore,
				ShippingCompany = 'OnTrac'

		-- Add by yangsub Lee on 08/22/2018, Add Log
		INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
			CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
			NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
		VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,@Address2,@State,
			'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', '0', '', '',
			@CartonID, @BannerName + '  ' + @Store+ ':' + @SubStore, 'OnTrac', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
	END
END
ELSE		-- UPS? FedEx? ?? ?? ZPL? ?? ????.
BEGIN
	
	DECLARE @Company VARCHAR(100)
	SET @Company = ISNULL((SELECT dbo.fnGetCodeFullName2('0008', LEFT(@TransportCodeName,4))),'')

	DECLARE @LogYN VARCHAR(2)
	IF EXISTS(SELECT 1 FROM dbo.tblCodeDetail WHERE Class = '0660' AND Code = @Store AND UseYN='Y')
		SET	@LogYN = ''
	ELSE
		SET	@LogYN = ' '

	DECLARE @Brand VARCHAR(100)
	SET @Brand = ''
	SELECT	@Brand = dbo.fnGetCodeFullName('0805', BannerID)
	FROM	dbo.tblStoreMapping 
	WHERE	StoreID = @Store AND SubStoreID = @SubStore

	DECLARE @ZplCode_NUPS VARCHAR(4000)

/*
	Live?? Print ??? ??? ?????.
	SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO' + CASE	WHEN @Type = '0' THEN '30,820^' + CASE WHEN @BarcodeSizeType='0' THEN 'BY3' ELSE 'BY4' END + '^B3N,N,' 
														WHEN @Type = '1' THEN '70,800^BY5^BCN,350,N,N,Y,A' 
														ELSE '30,820^' + CASE WHEN @BarcodeSizeType='0' THEN 'BY3' ELSE 'BY4' END + '^B3N,N,' END +						
						CASE WHEN @Type = '0' THEN '140' WHEN @Type = '1' THEN '310' ELSE '140' END + ',N,N
*/
	--IF Bucket? ??
	IF EXISTS(SELECT 1 FROM tblStore WHERE StoreID = @Store AND StoreClass = '0010')
	BEGIN
		SET @ZplCode_NUPS = '^XA' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,44^A0N,25,32^FD FOREVER 21 INC ^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,80^A0N,70,85^FD' + @Company + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO620,145^A0N,25,30^FD' + CASE WHEN @Type='1' THEN '@weight LB' ELSE CONVERT(VARCHAR,CONVERT(INT,@Weight))+' LBS' END +'^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,210^A0N,35,42^FDSHIP TO:^FS'  + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO120,250^A0N,30,37^FD' + @Brand + @LogYN + 'STORE #' + @Store + ':' + @SubStore + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO120,285^A0N,30,37^FD' + @Address + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO120,320^A0N,40,50^FD' + @ConsigneeCity + ', ' + @State + ' ' + @Zip1 + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,380^A0N,40,50^FD' + CONVERT(VARCHAR(10),GETDATE(),101) + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO100,460^A0N,100,125^FD' + @TruckNumber + '^FS' + CHAR(13)		
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO100,562^A0N,180,150^FD' + @Store + ':' + @SubStore + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO270,705^A0N,100,100^FD(' + @OldStore + ')^FS' + CHAR(13)	
		--SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO90,820^BY' + CASE WHEN LEN(@CartonID)=12 THEN '4' ELSE '3' END + '^A0N,40,30 ^BC,150,Y,N,N,A' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO' + CASE	WHEN @Type = '0' THEN '60,820^' + CASE WHEN @BarcodeSizeType='0' THEN 'BY3' ELSE 'BY4' END + CASE WHEN @BarcodeKind = 'Y' THEN '^BCN,' ELSE '^B3N,N,' END
														WHEN @Type = '1' THEN '60,800^BY3' +  CASE WHEN @BarcodeKind = 'Y' THEN '^BCN,350,Y,N,N,A' ELSE '^B3N,N,350,Y,N'  END 
														ELSE '60,820^' + CASE WHEN @BarcodeSizeType='0' THEN 'BY3' ELSE 'BY4' END + CASE WHEN @BarcodeKind = 'Y' THEN '^BCN,' ELSE '^B3N,N,' END END +						
						CASE WHEN @Type = '0' THEN '310' WHEN @Type = '1' THEN '310' ELSE '140' END + CASE WHEN @BarcodeKind = 'Y' THEN ',Y,N,N,A' ELSE ',Y,N' END
		--SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO70,820^BY' + '@BarcodeSize' + '^A0N,40,30 @BarcodeKind' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FD' + @CartonID + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO15,440^GB780,0,10,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO15,540^GB780,0,8,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO610,440^GB0,100,2,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO15,800^GB780,0,10,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^XZ'
	END
	ELSE
	BEGIN
		SET @ZplCode_NUPS = '^XA' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,44^A0N,25,32^FD FOREVER 21 INC ^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,80^A0N,70,85^FD' + @Company + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO620,145^A0N,25,30^FD' + CASE WHEN @Type='1' THEN '@weight LB' ELSE CONVERT(VARCHAR,CONVERT(INT,@Weight))+' LBS' END +'^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,210^A0N,35,42^FDSHIP TO:^FS'  + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO120,250^A0N,30,37^FD' + @Brand + @LogYN + 'STORE #' + @Store + ':' + @SubStore + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO120,285^A0N,30,37^FD' + @Address + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO120,320^A0N,40,50^FD' + @ConsigneeCity + ', ' + @State + ' ' + @Zip1 + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO80,380^A0N,40,50^FD' + CONVERT(VARCHAR(10),GETDATE(),101) + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO100,460^A0N,100,125^FD' + @TruckNumber + '^FS' + CHAR(13)
		-- 03 : 07/05/2018		: Jinbeom.p		: ZPL Code128 / 39 Swap 		
		--SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO50,580^A0N,280,180^FD' + @Store + ':' + @SubStore + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO50,580^A0N,280,170^FD' + @Store + ':' + @SubStore + '^FS' + CHAR(13)
		--SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO90,820^BY' + CASE WHEN LEN(@CartonID)=12 THEN '4' ELSE '3' END + '^A0N,40,30 ^BC,150,Y,N,N,A' + CHAR(13)
		--SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO70,820^BY' + '@BarcodeSize' + '^A0N,40,30 @BarcodeKind' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO' + CASE	WHEN @Type = '0' THEN '60,820^' + CASE WHEN @BarcodeSizeType='0' THEN 'BY3' ELSE 'BY4' END + CASE WHEN @BarcodeKind = 'Y' THEN '^BCN,' ELSE '^B3N,N,' END
														WHEN @Type = '1' THEN '60,800^BY3' + CASE WHEN @BarcodeKind = 'Y' THEN '^BCN,350,Y,N,N,A' ELSE '^B3N,N,350,Y,N'  END 
														ELSE '60,820^' + CASE WHEN @BarcodeSizeType='0' THEN 'BY3' ELSE 'BY4' END + CASE WHEN @BarcodeKind = 'Y' THEN '^BCN,' ELSE '^B3N,N,' END END +						
						CASE WHEN @Type = '0' THEN '310' WHEN @Type = '1' THEN '310' ELSE '140' END + CASE WHEN @BarcodeKind = 'Y' THEN ',Y,N,N,A' ELSE ',Y,N' END		
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FD' + @CartonID + '^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO15,440^GB780,0,10,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO15,540^GB780,0,8,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO610,440^GB0,100,2,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^FO15,800^GB780,0,10,B,0^FS' + CHAR(13)
		SET @ZplCode_NUPS = @ZplCode_NUPS + '^XZ'
	END


	SELECT	ReceiveName		= @Store,
			ReceiveSubName  = @SubStore,
			Organization	= '',
			city			= @ConsigneeCity,
			line			= @Address,
			line2			= '',
			state			= @State,
			countryCode		= 'US',
			countryName		= 'United States',
			postalCode		= @Zip1,
			tel				= @TelNo,
			mobile			= '',
			email			= 'Store' + @Store + '@Forever21.com',
			ResultStatus	= '1',
			ResultZPL		= @ZplCode_NUPS,
			FedexTrackingNo	= '',		
			NewCartonID		= @CartonID,		
			BannerName		= @Store + '  ',
			ShippingCompany = 'Other'
	
	-- Add by yangsub Lee on 08/22/2018, Add Log
	INSERT INTO tblFedExShipmentScanLog(ScanBarcode,ToStoreID,SubStoreID,Organization,City,Address1,Address2,State,
		CountryCode,CountryName,ZipCode,Tel,Mobile,Email,ResultStatus,ResultZPL,FedExTracKingNo,
		NewCartonID,BannerName,ShippingCompany,ClientHostName,ClientIPAddress,CreateUserID)
	VALUES(@ScanBarcode, @Store, @SubStore,'',@ConsigneeCity,@Address,'',@State,
		'US','United States', @Zip1, @TelNo, @Type, 'Store' + @Store + '@Forever21.com', '1', @ZplCode_NUPS, '',
		@CartonID, @Store + '  ', 'Other', HOST_NAME(), CONVERT(VARCHAR(50), CONNECTIONPROPERTY('client_net_address')), @UserID)
END



--@F21SP>**************************************************************
--*	SP Name			:  spSetFedExDayEndInsert
--*	Ref example		:   spFedExAPIClass;1
--* Created  By		: Jinbeom.p
--* Created Date	: 5/17/2018    
--* Used by			: FedEx_Shipments
--*	Description		:  Save FedEx shipping label information
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description
--@/F21SP>**************************************************************
CREATE PROCEDURE [dbo].spSetFedExDayEndInsert            
	@OrderNumber VARCHAR(32)
	,@IsLive CHAR(1)
	,@HighestSeverity VARCHAR(24)
	,@FedexTrackingNumber VARCHAR(50)
	,@ShippingMethodName VARCHAR(50)
	,@ServiceType VARCHAR(32)
	,@PackageWeight VARCHAR(12)
	,@ImageType VARCHAR(12)
	,@ReceiveName VARCHAR(50)
	,@ReceiveOrganization VARCHAR(50)
	,@ReceiveLine1 VARCHAR(50)
	,@ReceiveLine2 VARCHAR(50)
	,@ReceiveCity VARCHAR(50)
	,@ReceiveState VARCHAR(12)
	,@ReceiveCountryCode VARCHAR(4)
	,@ReceiveCountryName VARCHAR(24)
	,@ReceivePostalCode VARCHAR(12)
	,@ReceiveTel VARCHAR(24)
	,@FormId VARCHAR(12)
	,@Barcode VARCHAR(300)
	,@BarcodeType VARCHAR(20)
	,@UrsaPrefixCode VARCHAR(30)
	,@UrsaSuffixCode VARCHAR(30)
	,@DestinationLocationId VARCHAR(30)
	,@AirportId VARCHAR(30)
	,@TransitTime VARCHAR(32)
	,@PickupCode VARCHAR(12)
	,@Machinable CHAR(1)
	,@UspsApplicationId VARCHAR(12)
	,@IsSuccess CHAR(1)
	,@NoticeCode VARCHAR(8)
	,@NoticeMessage NVARCHAR(1000)
	,@NoticeSeverity VARCHAR(24)
	,@NoticeSource VARCHAR(12)
	,@ErrorMessage NVARCHAR(1000)         
	,@LabelString NVARCHAR(max)          
	,@RegEmpId VARCHAR(32)        

AS  
SET NOCOUNT ON;
	
INSERT INTO [dbo].tblFedexDayEnd
	([OrderNumber]
	,[IsLive]
	,[HighestSeverity]
	,[FedexTrackingNumber]
	,[ShippingMethodName]
	,[ServiceType]
	,[PackageWeight]
	,[ImageType]
	,[ReceiveName]
	,[ReceiveOrganization]
	,[ReceiveLine1]
	,[ReceiveLine2]
	,[ReceiveCity]
	,[ReceiveState]
	,[ReceiveCountryCode]
	,[ReceiveCountryName]
	,[ReceivePostalCode]
	,[ReceiveTel]
	,[FormId]
	,[Barcode]
	,[BarcodeType]
	,[UrsaPrefixCode]
	,[UrsaSuffixCode]
	,[DestinationLocationId]
	,[AirportId]
	,[TransitTime]
	,[PickupCode]
	,[Machinable]
	,[UspsApplicationId]
	,[IsSuccess]
	,[NoticeCode]
	,[NoticeMessage]
	,[NoticeSeverity]
	,[NoticeSource]
	,[ErrorMessage]         
	,[LabelString]       
	,CreateUserID
	,CreateDate
	,UpdateUserID
	,UpdateDate
	)
     VALUES
	(@OrderNumber
	,@IsLive
	,@HighestSeverity
	,@FedexTrackingNumber
	,@ShippingMethodName
	,@ServiceType
	,@PackageWeight
	,@ImageType
	,LEFT(@ReceiveName,4)
	,@ReceiveOrganization
	,@ReceiveLine1
	,@ReceiveLine2
	,@ReceiveCity
	,@ReceiveState
	,@ReceiveCountryCode
	,@ReceiveCountryName
	,@ReceivePostalCode
	,@ReceiveTel
	,@FormId
	,@Barcode
	,@BarcodeType
	,@UrsaPrefixCode
	,@UrsaSuffixCode
	,@DestinationLocationId
	,@AirportId
	,@TransitTime
	,@PickupCode
	,@Machinable
	,@UspsApplicationId
	,@IsSuccess
	,@NoticeCode
	,@NoticeMessage
	,@NoticeSeverity
	,@NoticeSource
	,@ErrorMessage      
	,@LabelString          
	,@RegEmpId
	,GETDATE()	
	,@RegEmpId
	,GETDATE()	
	)

--@F21SP>**************************************************************
--*	SP Name			:  spSetFedExDayEndErrorInsert
--*	Exe example		:  spFedExAPIClass;2
--* Created  By		: Jinbeom.p
--* Created Date	: 5/17/2018    
--* Used by			: FedEx_Shipments
--*	Description		:  Save FedEx WebService Error Log
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description
--@/F21SP>**************************************************************
CREATE	PROCEDURE [dbo].spSetFedExDayEndErrorInsert
	@OrderNumber VARCHAR(32),
	@ServiceType VARCHAR(32),
	@PackageWeight VARCHAR(12),
	@IsLive CHAR(1),
	@CountryCode VARCHAR(4),
	@LanguageCode VARCHAR(4),
	@LocaleCode VARCHAR(4),
	@Currency VARCHAR(4),
	@WeightUnit VARCHAR(4),
	@DimensionsUnit VARCHAR(4),
	@ImageType VARCHAR(10),
	@FormatType VARCHAR(100),
	@StockType VARCHAR(100),
	@PrintingOrientation VARCHAR(100),
	@isSuccess CHAR(1),
	@ResultCode VARCHAR(8),
	@ResultMessage NVARCHAR(1000),
	@ErrorMessage NVARCHAR(1000),
	@RegEmpId VARCHAR(32)
AS
SET NOCOUNT ON;
	
INSERT INTO [dbo].tblFedexDayEndErrorLog
	([OrderNumber]
	,[ServiceType]
	,[PackageWeight]
	,[IsLive]
	,[CountryCode]
	,[LanguageCode]
	,[LocaleCode]
	,[Currency]
	,[WeightUnit]
	,[DimensionsUnit]
	,[ImageType]
	,[FormatType]
	,[StockType]
	,[PrintingOrientation]
	,[isSuccess]
	,[ResultCode]
	,[ResultMessage]
	,[ErrorMessage]
	,CreateUserID
	,CreateDate)			   
VALUES (@OrderNumber
	,@ServiceType
	,@PackageWeight
	,@IsLive
	,@CountryCode
	,@LanguageCode
	,@LocaleCode
	,@Currency
	,@WeightUnit
	,@DimensionsUnit
	,@ImageType
	,@FormatType
	,@StockType
	,@PrintingOrientation
	,@isSuccess
	,@ResultCode
	,@ResultMessage
	,@ErrorMessage
	,@RegEmpId
	,GETDATE()
	)
			   
			   
--<F21SP>**************************************************************
--*        SP Name        : spSetFedExSCMCartonWeight
--*        Exe example    : EXEC spKR_UPSIntegration;108
--*        Created  By    : Jinbeom.p
--*        Created Date   : 05/17/2018
--*        Used by        : UPS Integration/modPrintLabel/SaveWeight
--*        Description    : Save weight of carton
-----------------------------------------------------------------------
-- No :   Date Modified   : Developer  : Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spSetFedExSCMCartonWeight
	@DataOwnerID	VARCHAR(4) = '0000',
	@CartonID		VARCHAR(19),
	@Transport		VARCHAR(4) = '',
	@Weight			SMALLINT,
	@PrintTime		VARCHAR(19),
	@Width			DECIMAL(10,2),
	@Length			DECIMAL(10,2),
	@Height			DECIMAL(10,2)
AS
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

IF EXISTS(
		SELECT	CartonID
		FROM	dbo.tblCartonWeight
		WHERE	CartonID = @CartonID
		)
BEGIN
	UPDATE	dbo.tblCartonWeight
	SET		Weight = @Weight, PrintTime = @PrintTime,  Width = @Width, Length = @Length, Height = @Height, UpdateUserID = 'PandaSys', UpdateDate = GETDATE()
	WHERE	CartonID = @CartonID
		    
END
ELSE
BEGIN
	IF ISNULL(@CartonID,'') != ''
	BEGIN
		IF @Transport = ''
			SET @Transport = (SELECT CASE WHEN LEN(Transport) = 0 THEN Transport2 ELSE Transport END FROM dbo.tblStoreAddInfo WHERE StoreID = SUBSTRING(@CartonID,3,4))

		INSERT	dbo.tblCartonWeight (DataOwnerID, CartonID, Transport, Weight, PrintTime, Width, Length, Height, CreateUserID, CreateDate, UpdateUserID, UpdateDate)
		VALUES ('0000', @CartonID, @Transport, @Weight, @PrintTime, @Width, @Length, @Height,'PandaSys',GETDATE(),'PandaSys',GETDATE())
	END
END	

--<F21SP>**************************************************************
--*	SP Name			: spSetFedExDayEndPrint
--*	Ref example		: spFedExAPIClass;4
--* Created  By		: minkyu Ryu
--* Created Date	: 8/16/2016    
--* Used by			: FedEx_Shipments
--*	Description		: Update FilePath
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE spSetFedExDayEndPrint
	@OrderNumber	VARCHAR(32),
	@FilePath		VARCHAR(50)   ,
	@TrackingNumber		VARCHAR(50)     
AS
SET NOCOUNT ON;
UPDATE	dbo.tblFedexDayEnd
SET		FilePath	= @FilePath,
        PrintYN		= 'Y',
		PrintDate	= GETDATE(),
		CreateDate = GETDATE(),
		UpdateDate = GETDATE()
WHERE	OrderNumber	= @OrderNumber

--<F21SP>**************************************************************    
--* SP Name		 : spSetOntracDayEndPrint
--* Ref example  : exec spOnTracAPIClass;2
--* Created  By  : Jinbeom.p
--* Created Date : 3/14/2017
--* Used by		 : FedEx_Shipments
--* Description  : insert OnTracHistory
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROC [dbo].spSetOntracDayEndPrint(
	@OrderNumber	VARCHAR(50),
	@FilePath		VARCHAR(50),
	@TrackingNumber VARCHAR(50)
)
AS
SET NOCOUNT ON;

UPDATE dbo.tblOnTracDayEnd
SET PrintYN = 'Y', 
	PrintDate = GETDATE(),
	FilePath = @FilePath,
	CreateDate =GETDATE(),
	UpdateDate = GETDATE()
WHERE OrderNumber = @OrderNumber

--<F21SP>**************************************************************    
--* SP Name		 : spSetOntracDayEndRePrint
--* Exe example  : exec spOnTracAPIClass;3
--* Created  By  : Jinbeom.p
--* Created Date : 5/17/2018
--* Used by		 : FedEx_Shipments
--* Description  : insert OnTracHistory
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROC [dbo].spSetOntracDayEndRePrint(
	@OrderNumber VARCHAR(50)
)
AS
SET NOCOUNT ON;

UPDATE dbo.tblOnTracDayEnd
SET RePrintYN = 'Y', 
	RePrintDate = GETDATE(),
	CreateDate = GETDATE(),
	UpdateDate = GETDATE()
WHERE OrderNumber = @OrderNumber

--<F21SP>**************************************************************
--*	SP Name			: spSetFedExDayEndRePrint
--*	Ref example		:  spFedExAPIClass;6
--* Created  By		: Jinbeom.p
--* Created Date	: 5/17/2018    
--* Used by			: FedEx_Shipments
--*	Description		:  Update Re-Print
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spSetFedExDayEndRePrint
	@OrderNumber	VARCHAR(32)   
AS
SET NOCOUNT ON;  
  
UPDATE	dbo.tblFedexDayEnd
SET		RePrintYN		= 'Y',
		RePrintDate		= GETDATE(),
		CreateDate = GETDATE(),
		UpdateDate = GETDATE()
WHERE	OrderNumber		= @OrderNumber

--<F21SP>***************************************************************
--* SP Name			: spGetFedExZPLReplace
--* Exe example		: EXEC spFedExZPLReplace
--* Created  By		: jinbeom.p
--* Created Date	: 05/17/2018
--* Used by			: FedEx ZPL Replace
--* Description		: FedEx ZPL code를 New로 Replace하는 list
-----------------------------------------------------------------------
-- No : Date Modified	: Developer     : Description
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetFedExZPLReplace
AS
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

SELECT	Seq, OldZPL, NewZPL
FROM	dbo.tblFedExZPLReplace


--<F21SP>***************************************************************
--* SP Name			: spGetOnTracZPLReplace
--* Exe example		: EXEC [spOnTracZPLReplace]
--* Created  By		: Jinbeom.p
--* Created Date	: 05/17/2018
--* Used by			: OnTrac ZPL Replace
--* Description		: OnTrac ZPL code를 New로 Replace하는 list
-----------------------------------------------------------------------
-- No : Date Modified	: Developer     : Description
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetOnTracZPLReplace
AS
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

SELECT	Seq, OldZPL, NewZPL
FROM	dbo.tblOnTracZPLReplace


--<F21SP>**************************************************************    
--* SP Name		 :spGetOnTracShippingWebService
--* Exe example  : exec spOnTracShippingWebService 'US', 'Y', 'GENERAL'
--* Created  By  : Jinbeom.p
--* Created Date : 5/17/2018
--* Used by		 : FedEx_Shipments
--* Description  : OnTrac WebService Account
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetOnTracShippingWebService
	@Country	VARCHAR(10),    
	@isLive		VARCHAR(2),  
	@AccType	VARCHAR(10) = 'GENERAL'  
AS    
SET NOCOUNT ON;    
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;    
    
	SELECT	*    
	FROM	dbo.tblOnTracShipService WITH(NOLOCK)    
	WHERE	Country = @Country    
	AND		isLive	= @isLive    
	AND		AccType = @AccType  

--<F21SP>**************************************************************    
--* SP Name		 : spGetOnTracShippingDetail
--* Exe example  : exec spOnTracShippingWebService;2 '6n12000192915'
--* Created  By  : Jinbeom.p
--* Created Date : 5/17/2018
--* Used by		 : FedEx_Shipments
--* Description  : OnTrac WebService DetailInfo
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetOnTracShippingDetail
	@CartonID	VARCHAR(50)
AS    
SET NOCOUNT ON;    
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;    

DECLARE @NewCartonID		VARCHAR(19)
DECLARE	@Store				VARCHAR(4)
	
--New CartonID 조회
EXEC spGetFedExScanCarton '0000',@CartonID, @NewCartonID OUTPUT

IF LEN(@NewCartonID) != 0
	SET @CartonID = @NewCartonID
ELSE 
	RETURN;
    
SET @Store = SUBSTRING(@CartonID,3,4)
    
SELECT	*    
FROM	dbo.tblOnTracShippingDetail WITH(NOLOCK)    
WHERE	Store = @Store



Text
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------




--<F21SP>**************************************************************    
--* SP Name		 : spSetOnTracDayEndInsert
--* Ref example  : spOnTracAPIClass
--* Created  By  : Jinbeom.p
--* Created Date : 5/17/2018
--* Used by		 : FedEx_Shipments
--* Description  : insert OnTracHistory
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROC [dbo].spSetOnTracDayEndInsert(
	@OrderNumber			VARCHAR(32),
    @IsLive					VARCHAR(1),
    @OnTracTrackingNumber	VARCHAR(50),
    @ShippingMethodName		VARCHAR(50),
    @ServiceType			VARCHAR(50),
    @PackageWeight			VARCHAR(12),
    @ImageType				VARCHAR(10),
    @ReceiveName			VARCHAR(50),
    @ReceiveOrganization	VARCHAR(50),
    @ReceiveLine1			VARCHAR(50),
    @ReceiveLine2			VARCHAR(50),
    @ReceiveCity			VARCHAR(50),
    @ReceiveState			VARCHAR(12),
    @ReceiveCountryCode		VARCHAR(4),
    @ReceiveCountryName		VARCHAR(24),
    @ReceivePostalCode		VARCHAR(12),
    @ReceiveTel				VARCHAR(24),
    @LabelString			VARCHAR(MAX),
    @TransitDays			INT,
    @ExpectedDeliveryDate	DATE,
    @CommitTime				TIME,
    @ServiceChrg			FLOAT,
    @BaseCharge				FLOAT,
    @CODCharge				FLOAT,
    @DeclaredCharge			FLOAT,
    @AdditionalCharges		FLOAT,
    @SaturdayCharge			FLOAT,
    @FuelChrg				FLOAT,
    @TotalChrg				FLOAT,
    @TariffChrg				FLOAT,
    @SortCode				VARCHAR(5),
    @RateZone				INT,
    @SignatureRequired		VARCHAR(1),
    @SaturdayRequired		VARCHAR(1),
    @Residential			VARCHAR(1),
    @Instruction			VARCHAR(100),
	@RegEmpId				VARCHAR(32),
	@IsSuccess				VARCHAR(1),
	@Reference1				VARCHAR(20) = ''
)
AS
SET NOCOUNT ON;

--beg temporary remove it later
IF @Reference1 = ''
BEGIN
	DECLARE @HostName VARCHAR(100)
	--SELECT top 1 @HostName = hostname FROM Sys.sysprocesses where spid = @@SPID
	SET @HostName = HOST_NAME()
		
	INSERT INTO tblFedExShipmentHostName (HostName,CreateUserID, CreateDate)
	VALUES (@HostName, @RegEmpId, GETDATE())
END
--end temporary
INSERT INTO dbo.tblOnTracDayEnd 
	(OrderNumber,
	IsLive,
	OnTracTrackingNumber,
	ShippingMethodName,
	PackageWeight,
	ImageType,
	ReceiveName,
	ReceiveOrganization,
	ReceiveLine1,
	ReceiveLine2,
	ReceiveCity,
	ReceiveState,
	ReceiveCountryCode,
	ReceiveCountryName,
	ReceivePostalCode,
	ReceiveTel,
	LabelString,
	TransitDays,
	ExpectedDeliveryDate,
	CommitTime,
	ServiceChrg,
	BaseCharge,
	CODCharge,
	DeclaredCharge,
	AdditionalCharges,
	SaturdayCharge,
	FuelChrg,
	TotalChrg,
	TariffChrg,
	SortCode,
	RateZone,
	SignatureRequired,
	SaturdayRequired,
	Residential,
	Instruction,							 
	IsSuccess,
	Reference,
	CreateUserID,
	CreateDate,
	UpdateUserID,
	UpdateDate
	)
VALUES 
	(@OrderNumber,
	@IsLive,
	@OnTracTrackingNumber,
	@ShippingMethodName,
	@PackageWeight,
	@ImageType,
	@ReceiveName,
	@ReceiveOrganization,
	@ReceiveLine1,
	@ReceiveLine2,
	@ReceiveCity,
	@ReceiveState,
	@ReceiveCountryCode,
	@ReceiveCountryName,
	@ReceivePostalCode,
	@ReceiveTel,
	@LabelString,
	@TransitDays,
	@ExpectedDeliveryDate,
	@CommitTime,
	@ServiceChrg,
	@BaseCharge,
	@CODCharge,
	@DeclaredCharge,
	@AdditionalCharges,
	@SaturdayCharge,
	@FuelChrg,
	@TotalChrg,
	@TariffChrg,
	@SortCode,
	@RateZone,
	@SignatureRequired,
	@SaturdayRequired,
	@Residential,
	@Instruction,			
	@IsSuccess,
	@Reference1,
	@RegEmpId,
	GETDATE(),
	@RegEmpId,
	GETDATE()
	)

--<F21SP>**************************************************************    
--* SP Name		 : spGetFedExShippingWebServiceLive
--* Exe example  : exec spFedexShippingWebService;2
--* Created  By  : Jinbeom.p
--* Created Date : 5/17/2018    
--* Used by		 : FedEx_Shipments
--* Description  : FedEx WebService Account
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetFedExShippingWebServiceLive
AS    
SET NOCOUNT ON;    
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;    
    
	SELECT	Country
	FROM	dbo.tblFedexShipService WITH(NOLOCK)    
	WHERE	isLive	= 'Y'    
	

--<F21SP>**************************************************************
--*	SP Name			:  spSetFedExDayEndCloseUpdate
--*	Ref example		:  EXEC spFedExAPIClass;7
--* Created  By		: jinbeom.p
--* Created Date	: 5/17/2018    
--* Used by			: FedEx_Shipments > FedEx Close
--*	Description		: Update FedEx close data
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spSetFedExDayEndCloseUpdate
    @PickupTime		DATETIME,
    @CloseType		VARCHAR(12),
	@CloseEmpId		VARCHAR(32)
AS
SET NOCOUNT ON;

DECLARE @PickupFrom DATETIME
DECLARE @PickupTo DATETIME

SET @PickupFrom = CAST(@PickupTime AS DATETIME)
SET @PickupTo = DATEADD("DAY", 1, @PickupTime)

UPDATE	dbo.tblFedExDayEnd 
SET		CloseDate	= GETDATE()
		,CloseEmpId	= @CloseEmpId
		,UpdateUserID = @CloseEmpId
		,UpdateDate = GETDATE()

WHERE	printDate	>= @PickupFrom	
	AND		PrintDate	< @PickupTo
	AND		isSuccess	= 'Y'
	AND		PrintYN		= 'Y'
	AND		LabelString <> '' 
	AND		LabelString IS NOT NULL
	AND		CloseDate IS NULL


--@F21SP>**************************************************************
--*	SP Name			:  spSetFedExDayEndCloseInsert
--*	Ref example		:  spFedExAPIClass;8
--* Created  By		: Jinbeom.p
--* Created Date	: 5/17/2018    
--* Used by			: FedEx_Shipments > FedEx Close
--*	Description		: Update FedEx close data
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description
--@/F21SP>**************************************************************
CREATE	PROCEDURE [dbo].spSetFedExDayEndCloseInsert
	@AccountNumber		VARCHAR(32),
	@IsLive				CHAR(1),
	@ServiceType		VARCHAR(32),
	@HighestSeverity	VARCHAR(24),
	@TransactionId		VARCHAR(50),
	@HubId				VARCHAR(6),
	@DestinationCountryCode VARCHAR(4),
	@IsSuccess			CHAR(1),
	@NoticeCode			VARCHAR(8),
	@NoticeMessage		VARCHAR(1000),
	@NoticeSeverity		VARCHAR(24),
	@NoticeSource		VARCHAR(12),
	@ErrorMessage		VARCHAR(1000),
	@FilePath			VARCHAR(200),
	@Version			VARCHAR(32),
	@IPAddress			VARCHAR(24),
	@StationNo			VARCHAR(12),		
	@ReqEmpId			VARCHAR(32)
AS
SET NOCOUNT ON; 

INSERT INTO dbo.tblFedExClose
	([AccountNumber]
	,[IsLive]
	,[ServiceType]
	,[HighestSeverity]
	,[TransactionId]
	,[HubId]
	,[DestinationCountryCode]
	,[IsSuccess]
	,[NoticeCode]
	,[NoticeMessage]
	,[NoticeSeverity]
	,[NoticeSource]
	,[ErrorMessage]
	,[FilePath]
	,[Version]
	,[IPAddress]
	,[StationNo]
	,CreateUserID
	,CreateDate
	,UpdateUserID
	,UpdateDate)
VALUES
	(@AccountNumber
	,@IsLive		
	,@ServiceType
	,@HighestSeverity
	,@TransactionId
	,@HubId
	,@DestinationCountryCode
	,@IsSuccess
	,@NoticeCode
	,@NoticeMessage
	,@NoticeSeverity
	,@NoticeSource
	,@ErrorMessage
	,@FilePath
	,@Version
	,@IPAddress
	,@StationNo           
	,@ReqEmpId
	,GETDATE()
	,@ReqEmpId
	,GETDATE()
	)

--<F21SP>**************************************************************    
--* SP Name		 : spGetFedExShippingWebService
--* Exe example  : exec spFedexShippingWebService 'US', 'Y', 'GENERAL'
--* Created  By  : Jinbeom.p
--* Created Date : 5/17/2018    
--* Used by		 : FedEx_Shipments
--* Description  : FedEx WebService Account
-----------------------------------------------------------------------
-- No :	Date Modified	: Developer		: Description 
--</F21SP>**************************************************************
CREATE PROCEDURE [dbo].spGetFedExShippingWebService
	@Country	VARCHAR(10),    
	@isLive		VARCHAR(2),  
	@AccType	VARCHAR(10) = 'GENERAL'  
AS    
SET NOCOUNT ON;    
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;    
    
SELECT	*    
FROM	dbo.tblFedexShipService
WHERE	Country = @Country    
	AND		isLive	= @isLive    
	AND		AccType = @AccType  
	












































