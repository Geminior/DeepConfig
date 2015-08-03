if not exists (select * from dbo.sysobjects where id = object_id(N'dbo.XmlConfigurationStore') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
begin
CREATE TABLE dbo.XmlConfigurationStore (
	configName varchar (50) NOT NULL ,
	configXml ntext NULL ,
	lastModified datetime NULL 
) 

ALTER TABLE dbo.XmlConfigurationStore ADD 
	CONSTRAINT PK_XmlConfigurationStore PRIMARY KEY  CLUSTERED 
	(
		configName
	)
end