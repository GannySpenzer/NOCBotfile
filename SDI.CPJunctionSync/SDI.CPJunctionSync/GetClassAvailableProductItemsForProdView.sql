select 
  cap.catalogID
, cap.productViewID
, cap.classID
, cap.itemID
, cap.customerItemID
from [{0}].dbo.ClassAvailableProducts cap
where cap.catalogID = {1}
  and cap.productViewID = {2}
order by cap.customerItemID, isnull(cap.updDate,cap.crDate) ASC
