-- Análise de impacto dos dígitos finais do CD_CLIENTE na tabela CORRWIN.TSCCLIBOL
-- Versão ALTAMENTE OTIMIZADA para 14 milhões de registros

-- CONSULTA 1: Distribuição estatística por dígitos (consulta matemática - EXTREMAMENTE RÁPIDA)
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */ 
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

-- CONSULTA 2: Análise para 1 dígito (todos os valores possíveis)
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */ 
           COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
)
SELECT 
    1 AS qtd_digitos,
    ultimo_digito AS valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 2) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM (
    SELECT /*+ NO_MERGE FULL(t) */
           SUBSTR(CD_CLIENTE, -1) AS ultimo_digito
    FROM CORRWIN.TSCCLIBOL t
)
GROUP BY ultimo_digito
ORDER BY ultimo_digito;

-- CONSULTA 3: Amostra para dígitos específicos (2 a 5 dígitos)
-- Versão muito mais eficiente que faz apenas uma passagem por tabela para cada grupo
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */ 
           COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
)
-- 2 dígitos - exemplos específicos
SELECT 
    2 AS qtd_digitos,
    valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM (
    SELECT 
        /*+ NO_MERGE FULL(t) */
        CASE
            WHEN SUBSTR(CD_CLIENTE, -2) = '00' THEN '00'
            WHEN SUBSTR(CD_CLIENTE, -2) = '01' THEN '01'
            WHEN SUBSTR(CD_CLIENTE, -2) = '10' THEN '10'
            WHEN SUBSTR(CD_CLIENTE, -2) = '25' THEN '25'
            WHEN SUBSTR(CD_CLIENTE, -2) = '50' THEN '50'
            WHEN SUBSTR(CD_CLIENTE, -2) = '75' THEN '75'
            WHEN SUBSTR(CD_CLIENTE, -2) = '99' THEN '99'
        END AS valor_digito
    FROM CORRWIN.TSCCLIBOL t
)
WHERE valor_digito IS NOT NULL
GROUP BY valor_digito
ORDER BY valor_digito;

-- 3 dígitos - exemplos específicos
SELECT 
    3 AS qtd_digitos,
    valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM (
    SELECT 
        /*+ NO_MERGE FULL(t) */
        CASE
            WHEN SUBSTR(CD_CLIENTE, -3) = '000' THEN '000'
            WHEN SUBSTR(CD_CLIENTE, -3) = '100' THEN '100'
            WHEN SUBSTR(CD_CLIENTE, -3) = '500' THEN '500'
            WHEN SUBSTR(CD_CLIENTE, -3) = '999' THEN '999'
        END AS valor_digito
    FROM CORRWIN.TSCCLIBOL t
)
WHERE valor_digito IS NOT NULL
GROUP BY valor_digito
ORDER BY valor_digito;

-- 4 dígitos - exemplos específicos
SELECT 
    4 AS qtd_digitos,
    valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM (
    SELECT 
        /*+ NO_MERGE FULL(t) */
        CASE
            WHEN SUBSTR(CD_CLIENTE, -4) = '0000' THEN '0000'
            WHEN SUBSTR(CD_CLIENTE, -4) = '1000' THEN '1000'
            WHEN SUBSTR(CD_CLIENTE, -4) = '5000' THEN '5000'
            WHEN SUBSTR(CD_CLIENTE, -4) = '9999' THEN '9999'
        END AS valor_digito
    FROM CORRWIN.TSCCLIBOL t
)
WHERE valor_digito IS NOT NULL
GROUP BY valor_digito
ORDER BY valor_digito;

-- 5 dígitos - exemplos específicos
SELECT 
    5 AS qtd_digitos,
    valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 4) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM (
    SELECT 
        /*+ NO_MERGE FULL(t) */
        CASE
            WHEN SUBSTR(CD_CLIENTE, -5) = '00000' THEN '00000'
            WHEN SUBSTR(CD_CLIENTE, -5) = '10000' THEN '10000'
            WHEN SUBSTR(CD_CLIENTE, -5) = '50000' THEN '50000'
            WHEN SUBSTR(CD_CLIENTE, -5) = '99999' THEN '99999'
        END AS valor_digito
    FROM CORRWIN.TSCCLIBOL t
)
WHERE valor_digito IS NOT NULL
GROUP BY valor_digito
ORDER BY valor_digito;

-- CONSULTA 4: Versão ainda mais otimizada para 1 dígito - usa MOD para extrair último dígito
-- Esta consulta é extremamente eficiente em comparação com SUBSTR
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE NO_MERGE */ 
           COUNT(*) AS total_registros
    FROM CORRWIN.TSCCLIBOL
)
SELECT 
    1 AS qtd_digitos,
    ultimo_digito AS valor_digito,
    COUNT(*) AS total_afetados,
    ROUND(COUNT(*) * 100.0 / (SELECT total_registros FROM base_total), 2) AS percentual_afetados,
    (SELECT total_registros FROM base_total) AS total_base_completa
FROM (
    SELECT /*+ NO_MERGE FULL(t) */
           -- Extrai o último dígito de forma matemática com MOD
           -- Funciona apenas se CD_CLIENTE for numérico
           -- Se for alfanumérico, use a CONSULTA 2 acima
           TO_CHAR(MOD(TO_NUMBER(CD_CLIENTE), 10)) AS ultimo_digito
    FROM CORRWIN.TSCCLIBOL t
    WHERE REGEXP_LIKE(CD_CLIENTE, '^\d+$') -- Garante que é apenas números
)
GROUP BY ultimo_digito
ORDER BY ultimo_digito; 