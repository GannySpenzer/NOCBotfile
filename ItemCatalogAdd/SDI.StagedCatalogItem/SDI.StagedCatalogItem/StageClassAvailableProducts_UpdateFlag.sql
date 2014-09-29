update [SDI_CPlus_Extend].[dbo].[ClassAvailableProducts] 
set
  [updDate]				= GETDATE()
, [InActiveFlag]		= '{4}'
where catalogID = {0}
  and productViewID = {1}
  and classID = {2}
  and itemID = {3}