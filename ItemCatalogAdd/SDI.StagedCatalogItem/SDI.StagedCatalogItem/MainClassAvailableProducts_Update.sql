update [{0}].[dbo].[ClassAvailableProducts] 
set
  [customerItemID]		= '{5}'
, [updDate]				= GETDATE()
where catalogID = {1}
  and productViewID = {2}
  and classID = {3}
  and itemID = {4}