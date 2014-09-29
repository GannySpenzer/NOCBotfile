update sysadm.PS_ISA_CP_JUNCTION
set 
  ISA_CP_PROD_ID = '{2}'
, ISA_CP_TRIG_FLAG = '{3}'
, ISA_PRODVIEW_FLAG = '{4}'
, DT_TIMESTAMP = SYSDATE
where ISA_SITE_ID = '{0}'
  and INV_ITEM_ID = '{1}'
