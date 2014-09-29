update [SDI_CPlus_Extend].[dbo].[ClassAvailableProducts] 
set
  [productViewID]		= A.[productViewID]
, [catalogID]			= A.[catalogID]
, [classID]				= A.[classID]
, [itemID]				= A.[itemID]
, [mainMarkup]			= A.[mainMarkup]
, [freightMarkup]		= A.[freightMarkup]
, [otherMarkup]			= A.[otherMarkup]
, [promoPrice]			= A.[promoPrice]
, [promoEffectiveDate]	= A.[promoEffectiveDate]
, [promoExpiryDate]		= A.[promoExpiryDate]
, [crUser]				= A.[crUser]
, [crDate]				= A.[crDate]
, [updUser]				= A.[updUser]
, [updDate]				= A.[updDate]
, [customerItemID]		= A.[customerItemID]
, [msrepl_tran_version] = A.[msrepl_tran_version]
, [InActiveFlag]		= '{5}'
from [SDI_CPlus_Extend].[dbo].[ClassAvailableProducts] B
inner join [{0}].[dbo].[ClassAvailableProducts] A
  on A.catalogID = B.catalogID
 and A.productViewID = B.productViewID
 and A.classID = B.classID
 and A.itemID = B.itemID
where A.catalogID = {1}
  and A.productViewID = {2}
  and A.classID = {3}
  and A.itemID = {4}