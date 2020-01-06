 /* Log Result of QuantumView Query */
 Create table SDIX_UPS_QUANTUMVIEW_LOG (
	/* use if ORA 11 - ups_quantumview_id number(11,0) NOT NULL 	constraint pk_ups_quantumview_id primary key, */
	/* Use if ORA12C or higher - ups_quantumview_id number GENERATED ALWAYS AS IDENTITY, */
	ups_filename varchar2(500) null,
	po_id VARCHAR2(25) null,
	 po_id_options NVARCHAR2(250) null,
	isa_asn_track_no VARCHAR2(30) null,
	ups_sdi_match varchar2(45) null,
	ups_file_location varchar2(500) null, 
	user_message nvarchar2(1000) null, /* email message for user */
	ups_record_type Varchar2(10) null,
		ups_delivery_type Varchar2(10) null,
	dttm_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP 
);
/* Use triggers for auto increment, if Oracle version is Oracle11  
CREATE SEQUENCE sdix_seq_ups_quantumview_id START WITH 0;

CREATE OR REPLACE TRIGGER sdix_trgr_ups_quantumview_id
	AFTER INSERT ON SDIX_UPS_QUANTUMVIEW_LOG  FOR EACH ROW
		BEGIN
			SELECT  sdix_seq_ups_quantumview_id.nextval INTO :new.ups_quantumview_id FROM dual;
		END;
		  */

 Create table SDIX_UPS_QUANTUMVIEW_ERROR (
	/* use if ORA 11 - ups_quantumview_error_id number(11,0) NOT NULL 	constraint pk_ups_quantumview_id primary key, */
	/* Use if ORA12C or higher - ups_quantumview_error_id number GENERATED ALWAYS AS IDENTITY, */
	error varchar2(1000) null,  
	dttm_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP 
);

CREATE PROCEDURE SDIX_UPS_QUANTUMVIEW_FIND_TRACKNUM_POID  (
	TRACKNUM in VARCHAR2, 
	POID in VARCHAR2) 
AS

   POID1 VARCHAR2;
   POID3 VARCHAR2;
   POID4 VARCHAR2;
   POID5 VARCHAR2;
   POID6 VARCHAR2;
   
   POID1 = REPLACE(POID, ' ');
   POID2 = REPLACE(POID, '#');
   POID3 = REPLACE(POID, '# ');
   POID4 = REPLACE(POID, 'PO');
   POID5 = REPLACE(POID, 'PO#');
   POID6 = REPLACE(POID, 'PO# ');
   
   BEGIN 
    SELECT DISTINCT  
                                    PO.business_unit as PO_BUSINESS_UNIT, 
                                    PO.PO_ID as PO_ID, 
                                    PO.PO_DT as PO_DT, 
                                    PO.VENDOR_ID as VENDOR_ID,  
                                    PO.BUYER_ID as BUYER_ID,   
                                    SH.BUSINESS_UNIT as SH_BUSINESS_UNIT, 
                                    SH.PO_ID as SH_PO_ID,  
                                    SH.ISA_ASN_TRACK_NO as SH_TRACK_NO,  
                                    ISA_ASN_SHIP_DT as SH_SHIP_DT,  
                                    SH.LINE_NBR as SH_LINE_NBR,  
                                    SH.SCHED_NBR as SH_SCHED_NBR,  
                                    SH.OPRID as SH_OPRID,   
                                    COM.NOTES_1000 as NOTES_1000   
                                FROM PS_PO_HDR PO  
                                    LEFT JOIN PS_ISA_ASN_SHIPPED SH ON PO.PO_ID = SH.PO_ID  
                                    LEFT JOIN PS_ISA_XPD_COMMENT COM ON PO.PO_ID = COM.PO_ID AND
									 COM.BUSINESS_UNIT = SH.BUSINESS_UNIT AND COM.LINE_NBR=SH.LINE_NBR AND COM.SCHED_NBR=SH.SCHED_NBR AND COM.OPRID=SH.OPRID    
                                    WHERE  
                                    (TRIM(SH.ISA_ASN_TRACK_No) = TRACKNUM   OR  
                                    TRIM(PO.PO_ID) = POID OR TRIM(PO.PO_ID)=  POID2 OR  
                                    TRIM(PO.PO_ID) = POID3 OR TRIM(PO.PO_ID) = POID4   OR   
                                    TRIM(PO.PO_ID) = POID5  OR TRIM(PO.PO_ID) = POID5   OR   
                                     TRIM(PO.PO_ID) = POID6) and rownum = 1
									 ORDER BY SH.ISA_ASN_SHIP_DT DESC ;
									 
END;

