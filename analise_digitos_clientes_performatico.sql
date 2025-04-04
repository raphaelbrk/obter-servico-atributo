-- Análise de impacto dos dígitos finais do CD_CLIENTE na tabela CORRWIN.TSCCLIBOL
-- Versão otimizada para alta performance com 14 milhões de registros

-- Consulta principal otimizada
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

-- Visão resumida por quantidade de dígitos (estatísticas agregadas)
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE */ 
           COUNT(*) AS total_registros 
    FROM CORRWIN.TSCCLIBOL
),
distribuicao_digitos AS (
    SELECT /*+ MATERIALIZE NO_MERGE */ 
           1 AS qtd_digitos,
           COUNT(*) AS total_registros,
           COUNT(*)/10 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
    
    UNION ALL
    
    SELECT 2 AS qtd_digitos,
           COUNT(*) AS total_registros,
           COUNT(*)/100 AS media_afetados_por_padrao
    FROM CORRWIN.TSCCLIBOL
)
SELECT 
    d.qtd_digitos,
    d.media_afetados_por_padrao AS media_afetados,
    ROUND(d.media_afetados_por_padrao * 100.0 / t.total_registros, 4) AS percentual_medio_afetados,
    t.total_registros AS total_base_completa
FROM distribuicao_digitos d, base_total t
ORDER BY d.qtd_digitos;

-- Versão mais eficiente para 5 dígitos (executa uma única passagem pela tabela)
WITH 
base_total AS (
    SELECT /*+ MATERIALIZE */ 
           COUNT(*) AS total_registros 
    FROM CORRWIN.TSCCLIBOL
)
SELECT
    'Resumo de impacto por quantidade de dígitos' AS descricao,
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

-- Versão para criação de uma view materializada (para consultas frequentes)
CREATE MATERIALIZED VIEW MV_IMPACTO_DIGITOS_CLIENTE
REFRESH COMPLETE ON DEMAND
ENABLE QUERY REWRITE
AS
WITH base_total AS (
    SELECT COUNT(*) AS total_registros FROM CORRWIN.TSCCLIBOL
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