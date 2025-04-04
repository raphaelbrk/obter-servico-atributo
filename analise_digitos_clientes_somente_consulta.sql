-- Análise de impacto dos dígitos finais do CD_CLIENTE na tabela CORRWIN.TSCCLIBOL
-- Versão otimizada para alta performance com 14 milhões de registros
-- Apenas com consultas SELECT (sem criar objetos no banco)

-- Consulta 1: Análise por último dígito
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */ 
           COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
),
analise_por_digito AS (
    -- Análise para 1 dígito (mais eficiente sem SUBSTR para cada linha)
    SELECT 
        1 AS qtd_digitos,
        MOD(TO_NUMBER(REGEXP_SUBSTR(CD_CLIENTE, '[0-9]$')), 10) AS valor_digito
    FROM CORRWIN.TSCCLIBOL
    WHERE REGEXP_LIKE(CD_CLIENTE, '[0-9]$')
),
contagem_por_digito AS (
    SELECT /*+ MATERIALIZE NO_MERGE */
        qtd_digitos,
        valor_digito,
        COUNT(*) AS total_afetados
    FROM analise_por_digito
    GROUP BY qtd_digitos, valor_digito
)
SELECT 
    qtd_digitos,
    valor_digito,
    total_afetados,
    ROUND(total_afetados * 100.0 / (SELECT total_registros FROM base_total), 2) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM contagem_por_digito
ORDER BY qtd_digitos, valor_digito;

-- Consulta 2: Visão resumida estatística por quantidade de dígitos
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE */ 
           COUNT(*) AS total_registros 
    FROM CORRWIN.TSCCLIBOL
)
SELECT
    nivel.digitos AS qtd_digitos,
    CEIL(t.total_registros / POWER(10, nivel.digitos)) AS media_registros_por_padrao,
    ROUND(100.0 / POWER(10, nivel.digitos), 4) AS percentual_medio_afetados,
    t.total_registros AS total_base_completa
FROM
    (SELECT 1 AS digitos FROM DUAL
     UNION ALL SELECT 2 FROM DUAL
     UNION ALL SELECT 3 FROM DUAL
     UNION ALL SELECT 4 FROM DUAL
     UNION ALL SELECT 5 FROM DUAL) nivel,
    base_total t
ORDER BY
    nivel.digitos;

-- Consulta 3: Análise exata para 2 dígitos finais específicos (exemplo)
WITH
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */
           COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
),
analise_ultimos_2 AS (
    SELECT
        2 AS qtd_digitos,
        SUBSTR(CD_CLIENTE, -2) AS valor_digito,
        COUNT(*) AS total_afetados
    FROM CORRWIN.TSCCLIBOL
    WHERE REGEXP_LIKE(CD_CLIENTE, '[0-9]{2}$')
    GROUP BY SUBSTR(CD_CLIENTE, -2)
)
SELECT
    qtd_digitos,
    valor_digito,
    total_afetados,
    ROUND(total_afetados * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM analise_ultimos_2
WHERE valor_digito IN ('00', '01', '10', '25', '50', '75', '99')
ORDER BY valor_digito;

-- Consulta 4: Análise para 3, 4 e 5 dígitos com amostra de valores específicos
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */
           COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
)
SELECT 
    '3' AS qtd_digitos,
    SUBSTR(CD_CLIENTE, -3) AS valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM CORRWIN.TSCCLIBOL
WHERE SUBSTR(CD_CLIENTE, -3) IN ('000', '100', '500', '999')
GROUP BY SUBSTR(CD_CLIENTE, -3)

UNION ALL

SELECT
    '4' AS qtd_digitos,
    SUBSTR(CD_CLIENTE, -4) AS valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM CORRWIN.TSCCLIBOL
WHERE SUBSTR(CD_CLIENTE, -4) IN ('0000', '1000', '5000', '9999')
GROUP BY SUBSTR(CD_CLIENTE, -4)

UNION ALL

SELECT
    '5' AS qtd_digitos,
    SUBSTR(CD_CLIENTE, -5) AS valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM CORRWIN.TSCCLIBOL
WHERE SUBSTR(CD_CLIENTE, -5) IN ('00000', '10000', '50000', '99999')
GROUP BY SUBSTR(CD_CLIENTE, -5)
ORDER BY qtd_digitos, valor_digito; 