-- 0	configName
-- 1	configXml
begin transaction
if exists(select 1 from XmlConfigurationStore where configName = '{0}')
begin
	update 
		XmlConfigurationStore
	set 
		configXml = N'{1}',
		lastModified = getdate()
	where
		configName = '{0}'
end
else
begin
	insert into 
		XmlConfigurationStore 
		(
		configName,
		configXml,
		lastModified
		) 
	values 
		('{0}', N'{1}', getdate())
end

select lastModified from XmlConfigurationStore where configName = '{0}'
commit