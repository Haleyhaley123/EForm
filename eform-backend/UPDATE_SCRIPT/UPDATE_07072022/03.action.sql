insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD] [BS CHỈNH SỬA] Phiếu đánh giá nguy cơ di truyền trong sàng lọc ung thư vú' , N'NCCNBROV1POST', (select id from VisitTypeGroups where Code = 'OPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD] [VIEW] Phiếu đánh giá nguy cơ di truyền trong sàng lọc ung thư vú' , N'NCCNBROV1GET', (select id from VisitTypeGroups where Code = 'OPD'));

insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD] [VIEW] Thông tin khách hàng đánh giá nguy cơ ung thư' , N'RISKASSFORCANCERGET', (select id from VisitTypeGroups where Code = 'OPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD] [BS CHỈNH SỬA] Thông tin khách hàng đánh giá nguy cơ ung thư ' , N'RISKASSFORCANCERPOST', (select id from VisitTypeGroups where Code = 'OPD'));

insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD/IPD] [BS CHỈNH SỬA] Biên bản phối hợp với bệnh nhân/ gia đình xử lý thai chết lưu' , N'STILLBIRTHPOST', (select id from VisitTypeGroups where Code = 'IPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD/IPD] [VIEW] Biên bản phối hợp với bệnh nhân/ gia đình xử lý thai chết lưu' , N'STILLBIRTHGET', (select id from VisitTypeGroups where Code = 'IPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD/IPD] [XÁC NHẬN] Biên bản phối hợp với bệnh nhân/ gia đình xử lý thai chết lưu' , N'STILLBIRTHCFPOST', (select id from VisitTypeGroups where Code = 'IPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD/IPD] [BS CHỈNH SỬA] Biên bản phối hợp với bệnh nhân/ gia đình xử lý thai chết lưu' , N'STILLBIRTHPOST', (select id from VisitTypeGroups where Code = 'OPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD/IPD] [VIEW] Biên bản phối hợp với bệnh nhân/ gia đình xử lý thai chết lưu' , N'STILLBIRTHGET', (select id from VisitTypeGroups where Code = 'OPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD/IPD] [XÁC NHẬN] Biên bản phối hợp với bệnh nhân/ gia đình xử lý thai chết lưu' , N'STILLBIRTHCFPOST', (select id from VisitTypeGroups where Code = 'OPD'));

Insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][TẠO MỚI] Phiếu khám lâm sàng vú' , N'CBENCOPD', (select id from VisitTypeGroups where Code = 'OPD'));
Insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][UPDATE] Phiếu khám lâm sàng vú' , N'CBENUUPD', (select id from VisitTypeGroups where Code = 'OPD'));
Insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][CONFIRM] Phiếu khám lâm sàng vú' , N'CBENUCFOPD', (select id from VisitTypeGroups where Code = 'OPD'));
Insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][GETLIST] Phiếu khám lâm sàng vú' , N'CBENUGLOPD', (select id from VisitTypeGroups where Code = 'OPD'));
Insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][GETDETAIL] Phiếu khám lâm sàng vú' , N'CBENUDTOPD', (select id from VisitTypeGroups where Code = 'OPD'));
