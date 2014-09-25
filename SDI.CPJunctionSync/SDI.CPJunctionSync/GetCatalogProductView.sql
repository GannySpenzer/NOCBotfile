select 
  A.catalogId as CATALOG_ID
, A.productViewId as PRODUCT_VIEW_ID
from classAvailableProducts A
group by A.catalogId, A.productViewId 
order by A.catalogId, A.productViewId 
