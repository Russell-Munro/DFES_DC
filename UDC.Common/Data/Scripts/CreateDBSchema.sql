IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = object_id('[equ_dc_Connection]') and OBJECTPROPERTY(object_id, 'IsUserTable') = 1) BEGIN
	Create table [equ_dc_Connection]
	(
		[connectionID] Bigint Identity(1,1) NOT NULL, UNIQUE ([connectionID]),
		[Name] Varchar(255) NULL,
		[SourcePlatformCfg] Text NULL,
		[DestinationPlatformCfg] Text NULL,
		[GlobalSrcDestObjSyncState] Text NULL,
		[Enabled] Bit NULL,
		[DateCreated] Datetime NULL,
		[LastUpdated] Datetime NULL
	);
	Alter table [equ_dc_Connection] add Constraint [pk_equ_dc_Connection] Primary Key ([connectionID]);
END
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = object_id('[equ_dc_ConnectionRule]') and OBJECTPROPERTY(object_id, 'IsUserTable') = 1) BEGIN
	Create table [equ_dc_ConnectionRule]
	(
		[connectionRuleID] Bigint Identity(1,1) NOT NULL, UNIQUE ([connectionRuleID]),
		[connectionID] Bigint NOT NULL,
		[Name] Varchar(255) NULL,
		[SyncIntervalCron] Varchar(255) NULL,
		[LastExecuted] Datetime NULL,
		[LastExecutedStatus] Text NULL,
		[SourceContainerCfg] Text NULL,
		[DestinationContainerCfg] Text NULL,
		[FieldMappings] Text NULL,
		[SrcDestObjSyncState] Text NULL,
		[SourcePostSyncTasks] Text NULL,
		[DestinationPostSyncTasks] Text NULL,
		[Enabled] Bit NULL,
		[DateCreated] Datetime NULL,
		[LastUpdated] Datetime NULL
	);
	Create UNIQUE CLUSTERED Index [CX_ConnectionRule_Index] ON [equ_dc_ConnectionRule] ([connectionID] ,[connectionRuleID] );
	Alter table [equ_dc_ConnectionRule] add Constraint [pk_equ_dc_ConnectionRule] Primary Key ([connectionRuleID]);
END
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = object_id('[equ_dc_DataConnectorLog]') and OBJECTPROPERTY(object_id, 'IsUserTable') = 1) BEGIN
	Create table [equ_dc_DataConnectorLog]
	(
		[dataConnectorLogID] Bigint Identity(1,1) NOT NULL, UNIQUE ([dataConnectorLogID]),
		[connectionRuleID] Bigint NULL,
		[LogType] Integer NULL,
		[Action] Integer NULL,
		[Result] Integer NULL,
		[Source] Varchar(255) NULL,
		[Message] Text NULL,
		[Data] Text NULL,
		[DateCreated] Datetime NULL
	);
	Create UNIQUE CLUSTERED Index [CX_DataConnectorLog_Index] ON [equ_dc_DataConnectorLog] ([connectionRuleID] ,[dataConnectorLogID] );
	Create Index [NX_DataConnectorLog_Index] ON [equ_dc_DataConnectorLog] ([DateCreated] Desc);
	Alter table [equ_dc_DataConnectorLog] add Constraint [pk_equ_dc_DataConnectorLog] Primary Key ([dataConnectorLogID]);
END
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = object_id('[equ_dc_UIUser]') and OBJECTPROPERTY(object_id, 'IsUserTable') = 1) BEGIN
	Create table [equ_dc_UIUser]
	(
		[uiUserID] Bigint Identity(1,1) NOT NULL, UNIQUE ([uiUserID]),
		[sessionID] Varchar(100) NULL,
		[remoteUserId] Varchar(100) NULL,
		[Username] Varchar(255) NULL,
		[Email] Varchar(255) NULL,
		[TZOffsetMins] Integer NULL,
		[IsConnectionAdmin] Bit NULL,
		[IsSyncManager] Bit NULL,
		[ReferringApplication] Varchar(255) NULL,
		[ReferringApplicationURL] Varchar(255) NULL,
		[LastAccessed] Datetime NULL
	);
	Alter table [equ_dc_UIUser] add Primary Key ([uiUserID]);
END
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = object_id('[equ_dc_ApplicationState]') and OBJECTPROPERTY(object_id, 'IsUserTable') = 1) BEGIN
	Create table [equ_dc_ApplicationState]
	(
		[applicationStateID] Bigint Identity(1,1) NOT NULL, UNIQUE ([applicationStateID]),
		[Key] Varchar(255) NULL,
		[Value] Text NULL,
		[ValueBinary] Varbinary(Max) NULL,
		[DateCreated] Datetime NULL,
		[LastUpdated] Datetime NULL
	);
	Alter table [equ_dc_ApplicationState] add Primary Key ([applicationStateID]);
END
IF NOT EXISTS (SELECT OBJECT_NAME(OBJECT_ID) FROM sys.objects WHERE type_desc='FOREIGN_KEY_CONSTRAINT' AND OBJECT_NAME(parent_object_id)='equ_dc_ConnectionRule' AND OBJECT_NAME(OBJECT_ID)='Relationship1') BEGIN
	Alter table [equ_dc_ConnectionRule] add Constraint [Relationship1] foreign key([connectionID]) references [equ_dc_Connection] ([connectionID])  on update no action on delete no action;
END
IF NOT EXISTS (SELECT OBJECT_NAME(OBJECT_ID) FROM sys.objects WHERE type_desc='FOREIGN_KEY_CONSTRAINT' AND OBJECT_NAME(parent_object_id)='equ_dc_DataConnectorLog' AND OBJECT_NAME(OBJECT_ID)='Relationship2') BEGIN
	Alter table [equ_dc_DataConnectorLog] add Constraint [Relationship2] foreign key([connectionRuleID]) references [equ_dc_ConnectionRule] ([connectionRuleID])  on update no action on delete no action;
END