SELECT 
 BUSINESS_UNIT
,DEMAND_SOURCE
,SOURCE_BUS_UNIT
,ORDER_NO
,ORDER_INT_LINE_NO
,SCHED_LINE_NO
,INV_ITEM_ID
,SUM(QTY_REQUESTED) AS QTY_REQUESTED
,SUM(QTY_ALLOCATED) AS QTY_ALLOCATED
FROM (
SELECT 
 PICK.BUSINESS_UNIT
,PICK.DEMAND_SOURCE
,PICK.SOURCE_BUS_UNIT
,PICK.ORDER_NO
,PICK.ORDER_INT_LINE_NO
,PICK.SCHED_LINE_NO
,PICK.INV_ITEM_ID
,DEMANDINV.QTY_REQUESTED AS QTY_REQUESTED
,DEMANDPHYS.QTY_PICKED AS QTY_ALLOCATED
FROM  
 PS_DEMAND_INV DEMANDINV
,PS_DEMAND_PHYS_INV DEMANDPHYS
,PS_ISA_PICKING_INT PICK
WHERE 
      PICK.SOURCE_BUS_UNIT = ':KEY_SOURCE_BU' 
  AND PICK.ORDER_NO = ':KEY_ORDER_NO' 
  AND PICK.DEMAND_SOURCE = ':KEY_DEMAND_SOURCE' 
  AND PICK.SCHED_LINE_NO = ':KEY_SCHED_LINE_NO' 
  AND PICK.INV_ITEM_ID = ':KEY_INV_ITEM_ID' 
  AND DEMANDINV.BUSINESS_UNIT = PICK.BUSINESS_UNIT 
  AND DEMANDINV.DEMAND_SOURCE = PICK.DEMAND_SOURCE 
  AND DEMANDINV.SOURCE_BUS_UNIT = PICK.SOURCE_BUS_UNIT 
  AND DEMANDINV.ORDER_NO = PICK.ORDER_NO 
  AND DEMANDINV.ORDER_INT_LINE_NO = PICK.ORDER_INT_LINE_NO 
  AND DEMANDINV.SCHED_LINE_NO = PICK.SCHED_LINE_NO 
  AND DEMANDINV.INV_ITEM_ID = PICK.INV_ITEM_ID 
  AND DEMANDINV.DEMAND_LINE_NO = PICK.DEMAND_LINE_NO
  AND DEMANDPHYS.BUSINESS_UNIT = PICK.BUSINESS_UNIT
  AND DEMANDPHYS.DEMAND_SOURCE = PICK.DEMAND_SOURCE
  AND DEMANDPHYS.SOURCE_BUS_UNIT = PICK.SOURCE_BUS_UNIT
  AND DEMANDPHYS.ORDER_NO = PICK.ORDER_NO
  AND DEMANDPHYS.ORDER_INT_LINE_NO = PICK.ORDER_INT_LINE_NO
  AND DEMANDPHYS.SCHED_LINE_NO = PICK.SCHED_LINE_NO
  AND DEMANDPHYS.INV_ITEM_ID = PICK.INV_ITEM_ID
  AND DEMANDPHYS.DEMAND_LINE_NO = PICK.DEMAND_LINE_NO
  AND DEMANDPHYS.SEQ_NBR = PICK.SEQ_NBR
GROUP BY 
 PICK.BUSINESS_UNIT
,PICK.DEMAND_SOURCE
,PICK.SOURCE_BUS_UNIT
,PICK.ORDER_NO
,PICK.ORDER_INT_LINE_NO
,PICK.SCHED_LINE_NO
,PICK.INV_ITEM_ID
,DEMANDINV.QTY_REQUESTED
,DEMANDPHYS.QTY_PICKED
) Z 
GROUP BY 
 Z.BUSINESS_UNIT
,Z.DEMAND_SOURCE
,Z.SOURCE_BUS_UNIT
,Z.ORDER_NO
,Z.ORDER_INT_LINE_NO
,Z.SCHED_LINE_NO
,Z.INV_ITEM_ID
