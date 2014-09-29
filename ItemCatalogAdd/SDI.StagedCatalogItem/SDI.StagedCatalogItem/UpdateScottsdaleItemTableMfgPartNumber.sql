update dbo.ScottsdaleItemTable
set 
  manufacturerName = {2}
, manufacturerPartNumber = {3}
, updDate = GETDATE()
where classID = {0}
  and itemID = {1}