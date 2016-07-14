--select * from preferences where Parameter = 'DEFUseAltPictureViewer'
if exists (select 1 from preferences where Parameter = 'DEFUseAltPictureViewer')
update Preferences set ParameterValue = 'Y' where Parameter = 'DEFUseAltPictureViewer'
else
INSERT INTO [Preferences] VALUES('0000','DEFUseAltPictureViewer','Y','Use Alternative Picture Viewer')



