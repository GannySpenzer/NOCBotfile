﻿SELECT
  CP.ISA_SITE_ID
, CP.INV_ITEM_ID 
, CP.ISA_CP_PROD_ID
FROM SYSADM.PS_ISA_CP_JUNCTION CP
WHERE CP.ISA_SITE_ID = '{0}'