SELECT TOP 1 VW.*
FROM (
		SELECT 
		  A.catalogID
		, A.classID
		, A.itemID
		, 0 AS rank_id
		FROM [{1}].dbo.ClassAvailableProducts A
		WHERE A.productViewID = {2}
		  AND A.customerItemID = '{3}'
		UNION ALL
		SELECT 
		  A.catalogID
		, A.classID
		, A.itemID
		, 10 AS rank_id
		FROM [SDI_CPlus_Extend].dbo.ClassAvailableProducts A
		WHERE A.productViewID = {2}
		  AND A.customerItemID = '{3}'
		UNION ALL
		SELECT 
		  A.catalogID
		, A.classID
		, A.itemID
		, 20 as rank_id
		from [SDI_CPlus_Extend].dbo.ScottsdaleItemTable A
		where A.custom_5 = '{0}'
) VW
ORDER BY VW.rank_id


