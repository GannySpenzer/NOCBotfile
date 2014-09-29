select top 1 A.classID
from [SDI_CPlus_Extend].[dbo].[ClassAvailableProducts] A
where A.catalogID = {1}
  and A.productViewID = {2}
  and A.itemID = {3}