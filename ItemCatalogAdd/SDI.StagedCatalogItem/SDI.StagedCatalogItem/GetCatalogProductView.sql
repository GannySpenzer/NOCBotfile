select 
  A.classId as CATALOG_ID
, A.productViewId as PRODUCT_VIEW_ID
from productViews A
group by A.classId, A.productViewId 
order by A.classId, A.productViewId 
