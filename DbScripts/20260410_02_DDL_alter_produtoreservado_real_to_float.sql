/*
  Align ProdutoReservado numeric columns with SQL Server FLOAT(53).

  Why:
  - If the existing database was created with FLOAT columns (8-byte / double precision),
    but the EF model uses Single/REAL mappings, materialization can throw:
      InvalidCastException: Unable to cast object of type 'System.Double' to 'System.Single'
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[ProdutoReservado]', N'U') IS NOT NULL
BEGIN
    -- REAL (system_type_id = 59) -> FLOAT(53) (system_type_id = 62)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'desconto' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [desconto] FLOAT(53) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'valor_tarifa' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [valor_tarifa] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'valor_comissao' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [valor_comissao] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'valor_taxas' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [valor_taxas] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'valor_tarifa_origem' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [valor_tarifa_origem] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'cancel_fee_aplicado' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [cancel_fee_aplicado] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'total_imp_sob_taxas_tarifa' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [total_imp_sob_taxas_tarifa] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'total_imp_sob_supl' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [total_imp_sob_supl] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'total_impostos' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [total_impostos] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'total_suplementos' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [total_suplementos] FLOAT(53) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]') AND name = N'descontoAutomatico' AND system_type_id = 59)
        ALTER TABLE [dbo].[ProdutoReservado] ALTER COLUMN [descontoAutomatico] FLOAT(53) NOT NULL;
END
GO

