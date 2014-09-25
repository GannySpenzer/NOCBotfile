select count(*) as rec
from [{0}].dbo.ClassAvailableProducts cap
where cap.catalogID = {1}
  and cap.productViewID = {2}
  and cap.customerItemID = '{3}'
