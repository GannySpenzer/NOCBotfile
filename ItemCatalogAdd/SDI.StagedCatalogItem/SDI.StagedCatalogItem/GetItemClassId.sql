select top 1 A.classID
from [{0}].[dbo].[ClassAvailableProducts] A
where A.catalogID = {1}
  and A.productViewID = {2}
  and A.itemID = {3}