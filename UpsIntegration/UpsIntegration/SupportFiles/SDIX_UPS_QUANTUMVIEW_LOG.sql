Create table SDIX_UPS_QUANTUMVIEW_LOG (
	ups_filename varchar2(500) null, 
	po_id VARCHAR2(25) null, 
	po_id_options NVARCHAR2(250) null,
	 sender_type varchar2(50) null, 
	 isa_asn_track_no VARCHAR2(30) null, 
	 ups_sdi_match varchar2(45) null,
	ups_file_location varchar2(500) null,  user_message nvarchar2(1000) null,   ups_record_type Varchar2(10) null,
		ups_delivery_type Varchar2(10) null, dttm_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP 
)