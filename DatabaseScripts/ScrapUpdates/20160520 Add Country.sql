--select * from CodeTable where CodeName like  'STATES%'

update CodeTable set CodeDisp2 = 'USA' where CodeName = 'STATESUSA'
update CodeTable set CodeDisp2 = 'CAN' where CodeName = 'STATESCAN'
update CodeTable set CodeDisp2 = 'MEX' where CodeName = 'STATESMEX'
update CodeTable set CodeDisp2 = 'AU' where CodeName = 'STATESAU'