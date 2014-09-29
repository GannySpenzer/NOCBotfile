insert into [{0}].[dbo].[ClassAvailableProducts] (
  [productViewID] 
, [catalogID] 
, [classID] 
, [itemID] 
, [mainMarkup] 
, [freightMarkup] 
, [otherMarkup] 
, [promoPrice] 
, [promoEffectiveDate] 
, [promoExpiryDate] 
, [crUser] 
, [crDate] 
, [updUser] 
, [updDate] 
, [customerItemID] 
, [msrepl_tran_version] 
)
select 
  A.[productViewID] 
, A.[catalogID] 
, A.[classID] 
, A.[itemID] 
, A.[mainMarkup] 
, A.[freightMarkup] 
, A.[otherMarkup] 
, A.[promoPrice] 
, A.[promoEffectiveDate] 
, A.[promoExpiryDate] 
, A.[crUser] 
, A.[crDate] 
, A.[updUser] 
, GETDATE()
, A.[customerItemID] 
, A.[msrepl_tran_version] 
from [SDI_CPlus_Extend].[dbo].[ClassAvailableProducts] A
where A.catalogID = {1}
  and A.productViewID = {2}
  and A.classID = {3}
  and A.itemID = {4}