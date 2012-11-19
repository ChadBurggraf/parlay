DROP TABLE IF EXISTS [ParlayStatistics];
DROP TABLE IF EXISTS [ParlayItem];

CREATE TABLE [ParlayStatistics]
(
	[ItemCount] INTEGER NOT NULL,
	[Size] INTEGER NOT NULL
);

INSERT INTO [ParlayStatistics]([ItemCount],[Size])
VALUES(0,0);

CREATE TABLE [ParlayItem]
(
	[ExpireDate] DATETIME NULL,
	[FirstAccessDate] DATETIME NOT NULL,
	[Key] TEXT NOT NULL PRIMARY KEY,
	[LastAccessDate] DATETIME NOT NULL,
	[Size] INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_ParlayItem_LastAccessDate]
ON [ParlayItem]([LastAccessDate]);