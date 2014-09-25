select 
  a.isa_business_unit
, SUBSTRING(a.isa_business_unit,3,3) as isa_site_id
, a.PRODUCT_VIEW_ID
, a.[ID]
, a.SITE_ITEM_PREFIX PREFIX
, a.ISA_COMPANY_ID
, a.SITE_INDICATOR_FLAG
from SDI_CPlus_Extend.dbo.PS_ISA_ENTERPRISE_XREF a
where a.SITE_INDICATOR_FLAG IN ('A','S')
	and isnull(a.SITE_ITEM_PREFIX,'') <> ''
