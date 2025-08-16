-- Oracle Target Database Schema
CREATE TABLE PlcData (
    Id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    LineId VARCHAR2(50) NOT NULL,
    Data CLOB NOT NULL,
    SyncStatus NUMBER(1) DEFAULT 0,
    Timestamp TIMESTAMP DEFAULT SYSTIMESTAMP
);

CREATE INDEX idx_plcdata_sync ON PlcData(LineId, SyncStatus);