select 
  itm.setid AS SETID
, itm.inv_item_id AS INV_ITEM_ID
, substr(itm.inv_item_id,1,3) AS ITEM_PREFIX
, trim(substr(itm.inv_item_id,4,250)) AS CUSTOMER_ITEM_ID
, itm.effdt AS EFF_DATE
, itm.eff_status AS EFF_STATUS
, itm.inv_item_type AS ITEM_TYPE
, itm.inv_stock_type AS STOCK_TYPE
, itm.descr254 AS DESCR254
, itm.COMMODITY_CD AS ITEM_ID
, mitm.UNIT_MEASURE_STD as UM
, cpj.isa_site_id AS ISA_SITE_ID
, cpj.isa_cp_prod_id AS CATALOG_ITEM_ID
from sysadm.ps_inv_items itm
inner join sysadm.ps_master_item_tbl mitm
  on mitm.SETID = itm.SETID
 and mitm.INV_ITEM_ID = itm.INV_ITEM_ID
left outer join sysadm.ps_isa_cp_junction cpj
  on cpj.INV_ITEM_ID = itm.INV_ITEM_ID
where itm.setid = 'MAIN1'
  and itm.effdt between to_date('{0}','MM/DD/YYYY HH24:MI:SS') and to_date('{1}','MM/DD/YYYY HH24:MI:SS')
  and itm.effdt = (
  select max(itm2.effdt) from sysadm.ps_inv_items itm2
  where itm2.setid = itm.setid
    and itm2.inv_item_id = itm.inv_item_id
	and itm2.effdt <= to_date('{1}','MM/DD/YYYY HH24:MI:SS')
)
order by itm.INV_ITEM_ID
