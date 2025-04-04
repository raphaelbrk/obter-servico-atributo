-- Análise de impacto dos dígitos finais do CD_CLIENTE na tabela CORRWIN.TSCCLIBOL
-- Este script mostra quantos registros serão afetados ao filtrar por diferentes 
-- quantidades de dígitos finais do CD_CLIENTE

WITH 
-- Tabela base com total de registros
base_total AS (
    SELECT COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
),

-- Extrai os dígitos finais do CD_CLIENTE de 1 a 9 dígitos
digitos_finais AS (
    SELECT 
        CD_CLIENTE,
        SUBSTR(CD_CLIENTE, -1) AS ultimo_digito,
        SUBSTR(CD_CLIENTE, -2) AS ultimos_2_digitos,
        SUBSTR(CD_CLIENTE, -3) AS ultimos_3_digitos,
        SUBSTR(CD_CLIENTE, -4) AS ultimos_4_digitos,
        SUBSTR(CD_CLIENTE, -5) AS ultimos_5_digitos,
        SUBSTR(CD_CLIENTE, -6) AS ultimos_6_digitos,
        SUBSTR(CD_CLIENTE, -7) AS ultimos_7_digitos,
        SUBSTR(CD_CLIENTE, -8) AS ultimos_8_digitos,
        SUBSTR(CD_CLIENTE, -9) AS ultimos_9_digitos
    FROM CORRWIN.TSCCLIBOL
),

-- Calcula o impacto para cada conjunto de dígitos (0-9)
impacto_por_digito AS (
    SELECT 
        '1' AS qtd_digitos,
        d AS valor_digito,
        COUNT(*) AS total_afetados
    FROM digitos_finais
    CROSS JOIN (
        SELECT '0' AS d FROM DUAL UNION ALL
        SELECT '1' FROM DUAL UNION ALL
        SELECT '2' FROM DUAL UNION ALL
        SELECT '3' FROM DUAL UNION ALL
        SELECT '4' FROM DUAL UNION ALL
        SELECT '5' FROM DUAL UNION ALL
        SELECT '6' FROM DUAL UNION ALL
        SELECT '7' FROM DUAL UNION ALL
        SELECT '8' FROM DUAL UNION ALL
        SELECT '9' FROM DUAL
    ) digitos
    WHERE ultimo_digito = d
    GROUP BY d
    
    UNION ALL
    
    -- Para 2 dígitos, mostro apenas alguns exemplos (00, 01, 02...)
    SELECT 
        '2' AS qtd_digitos,
        d AS valor_digito,
        COUNT(*) AS total_afetados
    FROM digitos_finais
    CROSS JOIN (
        SELECT '00' AS d FROM DUAL UNION ALL
        SELECT '01' FROM DUAL UNION ALL
        SELECT '02' FROM DUAL UNION ALL
        SELECT '10' FROM DUAL UNION ALL
        SELECT '20' FROM DUAL UNION ALL
        SELECT '50' FROM DUAL UNION ALL
        SELECT '99' FROM DUAL
    ) digitos
    WHERE ultimos_2_digitos = d
    GROUP BY d
    
    UNION ALL
    
    -- Para 3 dígitos, apenas alguns exemplos
    SELECT 
        '3' AS qtd_digitos,
        d AS valor_digito,
        COUNT(*) AS total_afetados
    FROM digitos_finais
    CROSS JOIN (
        SELECT '000' AS d FROM DUAL UNION ALL
        SELECT '100' FROM DUAL UNION ALL
        SELECT '500' FROM DUAL UNION ALL
        SELECT '999' FROM DUAL
    ) digitos
    WHERE ultimos_3_digitos = d
    GROUP BY d
)

-- Resultado final mostrando impacto agregado por quantidade de dígitos
SELECT 
    i.qtd_digitos,
    i.valor_digito,
    i.total_afetados,
    ROUND(i.total_afetados * 100.0 / t.total_registros, 2) AS percentual_afetados,
    t.total_registros AS total_base_completa
FROM impacto_por_digito i
CROSS JOIN base_total t
ORDER BY i.qtd_digitos, i.valor_digito;

-- Visão resumida por quantidade de dígitos (média)
WITH 
base_total AS (
    SELECT COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
),
impacto_geral AS (
    SELECT 
        '1' AS qtd_digitos,
        COUNT(*)/10 AS media_afetados_por_padrao -- divide por 10 porque temos dígitos 0-9
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '2' AS qtd_digitos,
        COUNT(*)/100 AS media_afetados_por_padrao -- divide por 100 (00-99)
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '3' AS qtd_digitos,
        COUNT(*)/1000 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '4' AS qtd_digitos,
        COUNT(*)/10000 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '5' AS qtd_digitos,
        COUNT(*)/100000 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
)

SELECT 
    i.qtd_digitos,
    i.media_afetados_por_padrao AS media_afetados,
    ROUND(i.media_afetados_por_padrao * 100.0 / t.total_registros, 4) AS percentual_medio_afetados,
    t.total_registros AS total_base_completa
FROM impacto_geral i
CROSS JOIN base_total t
ORDER BY i.qtd_digitos;

-- Visão para criação permanente
CREATE OR REPLACE VIEW VW_IMPACTO_DIGITOS_CLIENTE AS
WITH 
base_total AS (
    SELECT COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
),
impacto_geral AS (
    SELECT 
        '1' AS qtd_digitos,
        COUNT(*)/10 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '2' AS qtd_digitos,
        COUNT(*)/100 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '3' AS qtd_digitos,
        COUNT(*)/1000 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '4' AS qtd_digitos,
        COUNT(*)/10000 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 
        '5' AS qtd_digitos,
        COUNT(*)/100000 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
)

SELECT 
    i.qtd_digitos,
    i.media_afetados_por_padrao AS media_afetados,
    ROUND(i.media_afetados_por_padrao * 100.0 / t.total_registros, 4) AS percentual_medio_afetados,
    t.total_registros AS total_base_completa
FROM impacto_geral i
CROSS JOIN base_total t
ORDER BY i.qtd_digitos; 