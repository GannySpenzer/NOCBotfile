select distinct 
  substr(A.isa_business_unit,3,3) as ISA_SITE_ID
, A.isa_cplus_prodview AS PRODUCT_VIEW_ID
from sysadm.ps_isa_enterprise A
where A.isa_cplus_prodview <> 0
  and A.isa_cplus_prodview = (
    select max(A2.isa_cplus_prodview) from sysadm.ps_isa_enterprise A2
    where substr(A2.isa_business_unit,3,3) = substr(A.isa_business_unit,3,3)
  )
order by substr(A.isa_business_unit,3,3), A.isa_cplus_prodview
