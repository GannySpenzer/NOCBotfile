@rem
@rem FTP -s:C:\SHIPXML\XMLOut260\DDPutXML.ftp
@rem FTP -s:C:\SHIPXML\ShipXMLIn\DDGetXML.ftp
@rem
cd "C:\Program Files\SDI\OrderStatus.EmailNotification"
"C:\Program Files\SDI\OrderStatus.EmailNotification\orderStatEmailOut.exe" /log=verbose /db=RPTG
