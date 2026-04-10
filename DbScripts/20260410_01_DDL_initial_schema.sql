/*
  Initial schema for api_aggregations

  Notes:
  - Targets Microsoft SQL Server
  - Uses "IF NOT EXISTS" guards to be re-runnable
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[Reserva]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Reserva]
    (
        [id] INT IDENTITY(1,1) NOT NULL,
        [numero] NVARCHAR(MAX) NULL,
        [tipo] TINYINT NULL,
        [estado] SMALLINT NULL,
        [data_pedido] DATETIME2 NULL,
        [data_anulacao] DATETIME2 NULL,
        [id_vendedor] INT NOT NULL,
        [referencia] NVARCHAR(MAX) NULL,
        [observacoes] NVARCHAR(MAX) NULL,
        [id_externo] NVARCHAR(MAX) NOT NULL,
        [sistema_externo] INT NOT NULL,
        [data_actualizacao] DATETIME2 NOT NULL,
        [estado_pagamento] TINYINT NOT NULL,
        [id_canal] INT NOT NULL,
        [nome_utilizador_confirmacao] NVARCHAR(MAX) NOT NULL,
        CONSTRAINT [PK_Reserva] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

IF OBJECT_ID(N'[dbo].[ProdutoReservado]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProdutoReservado]
    (
        [id] INT IDENTITY(1,1) NOT NULL,
        [DataInicio] DATETIME2 NULL,
        [DataFim] DATETIME2 NULL,
        [id_reserva] INT NOT NULL,
        [estado] SMALLINT NULL,
        [id_tarifa] INT NOT NULL,
        [id_produto] INT NOT NULL,
        [referencia] NVARCHAR(MAX) NULL,
        [numero] NVARCHAR(MAX) NULL,
        [agregado] TINYINT NULL,
        [id_produto_reservado] INT NULL,
        [hora_inicio] NVARCHAR(MAX) NULL,
        [hora_fim] NVARCHAR(MAX) NULL,
        [id_entidade] INT NULL,
        [n_pessoas] INT NULL,
        [quantidade] INT NOT NULL,
        [desconto] REAL NULL,
        [valor_tarifa] REAL NOT NULL,
        [valor_comissao] REAL NOT NULL,
        [valor_taxas] REAL NOT NULL,
        [ref_taxas] NVARCHAR(MAX) NOT NULL,
        [id_factura] INT NOT NULL,
        [valor_tarifa_origem] REAL NOT NULL,
        [tipo_tarifa] SMALLINT NULL,
        [data_cancelamento] DATETIME2 NOT NULL,
        [cancel_fee_aplicado] REAL NOT NULL,
        [data_actualizacao] DATETIME2 NOT NULL,
        [data_criacao] DATETIME2 NOT NULL,
        [checkin_hora] DATETIME2 NULL,
        [total_imp_sob_taxas_tarifa] REAL NOT NULL,
        [total_imp_sob_supl] REAL NOT NULL,
        [total_impostos] REAL NOT NULL,
        [total_suplementos] REAL NOT NULL,
        [id_externo] NVARCHAR(MAX) NOT NULL,
        [sistema_externo] INT NOT NULL,
        [refExterno] NVARCHAR(MAX) NOT NULL,
        [id_ponto_partida] INT NOT NULL,
        [id_ponto_consumo] INT NOT NULL,
        [id_ponto_destino] INT NOT NULL,
        [nome_produto] NVARCHAR(MAX) NOT NULL,
        [n_factura] NVARCHAR(MAX) NOT NULL,
        [obs_factura] NVARCHAR(MAX) NOT NULL,
        [id_disp_base] INT NOT NULL,
        [id_lugar] NVARCHAR(MAX) NOT NULL,
        [valor_comissao_suplementos] DECIMAL(18,2) NOT NULL,
        [UserConfirmacao] NVARCHAR(MAX) NULL,
        [UserCheckIn] NVARCHAR(MAX) NULL,
        [UserCancelamento] NVARCHAR(MAX) NULL,
        [IdUserConfirmacao] INT NULL,
        [IdUserCheckIn] INT NULL,
        [IdUserCancelamento] INT NULL,
        [DataCheckIn] DATETIME2 NULL,
        [DataConfirmacao] DATETIME2 NULL,
        [id_canal] INT NOT NULL,
        [id_codigoDesconto] INT NOT NULL,
        [data_embarque] DATETIME2 NOT NULL,
        [data_desembarque] DATETIME2 NOT NULL,
        [descontoAutomatico] REAL NOT NULL,
        CONSTRAINT [PK_ProdutoReservado] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_ProdutoReservado_Reserva_id_reserva'
)
BEGIN
    ALTER TABLE [dbo].[ProdutoReservado] WITH CHECK
    ADD CONSTRAINT [FK_ProdutoReservado_Reserva_id_reserva]
        FOREIGN KEY([id_reserva])
        REFERENCES [dbo].[Reserva] ([id])
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ProdutoReservado_id_reserva'
      AND object_id = OBJECT_ID(N'[dbo].[ProdutoReservado]')
)
BEGIN
    CREATE INDEX [IX_ProdutoReservado_id_reserva]
        ON [dbo].[ProdutoReservado]([id_reserva]);
END
GO

