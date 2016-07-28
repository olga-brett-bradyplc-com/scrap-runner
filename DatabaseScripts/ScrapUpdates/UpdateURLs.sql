update exp_view.a_DataService set Url = 'https://jacnb-jlc10.bradyplc.com:7776/api/scraprunner' where Url = 'https://localhost:7776/api/scraprunner'
update exp_view.a_DataService set Url = 'https://jacnb-jlc10.bradyplc.com:7776/api/explorer' where Url = 'https://localhost:7776/api/explorer'
update exp_view.a_DataService set Url = 'https://jacnb-jlc10.bradyplc.com:7776/api/membership' where Url = 'https://localhost:7776/api/membership'

select * from  exp_view.a_DataService

select * from  exp_view.a_DataServiceHost

update exp_view.DataService set ServiceAddress = 'https://jacnb-jlc10.bradyplc.com:7776/api/scraprunner' where ServiceAddress = 'https://localhost:7776/api/scraprunner'
update exp_view.DataService set ServiceAddress = 'https://jacnb-jlc10.bradyplc.com:7776/api/explorer' where ServiceAddress = 'https://localhost:7776/api/explorer'
update exp_view.DataService set ServiceAddress = 'https://jacnb-jlc10.bradyplc.com:7776/api/membership' where ServiceAddress = 'https://localhost:7776/api/membership'

select * from  exp_view.DataService

update exp_view.DataServiceHost set Url = 'https://jacnb-jlc10.bradyplc.com:7776' where Url = 'https://localhost:7776'

select * from  exp_view.DataServiceHost