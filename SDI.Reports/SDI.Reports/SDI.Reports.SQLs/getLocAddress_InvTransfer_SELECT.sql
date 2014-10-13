SELECT
 A.DESCR
,A.ADDRESS1
,A.ADDRESS2
,A.ADDRESS3
,A.ADDRESS4
,A.CITY
,A.STATE
,A.POSTAL   
FROM PS_LOCATION_TBL A
WHERE A.SETID = 'MAIN1'
  AND A.LOCATION = ':KEY_LOCATION'
  AND A.EFF_STATUS = 'A'
  AND A.EFFDT = (
                  SELECT MAX(A1.EFFDT)
                  FROM PS_LOCATION_TBL A1
                  WHERE A1.SETID = A.SETID
                    AND A1.LOCATION = A.LOCATION 
                    AND A1.EFF_STATUS = A.EFF_STATUS
                    AND A1.EFFDT <= SYSDATE
                )
