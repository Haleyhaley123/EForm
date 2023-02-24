using DataAccess.Models.EIOModel;
using DataAccess.Models.IPDModel;
using EForm.BaseControllers;
using EForm.Common;
using EForm.Models.IPDModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EForm.Controllers.BaseControllers.BaseEIOControllers
{
    public class EIOCareNoteController : BaseApiController
    {
        protected dynamic GetListEIOCareNote(Guid visit_id, string visit_type, string from, string to, string createdBy, int? sort = null, Guid? formId_newborn = null)
        {
            var start = string.IsNullOrEmpty(from) ? (DateTime?)null : DateTime.ParseExact(from, Constant.TIME_DATE_FORMAT_WITHOUT_SECOND, null);
            var end = string.IsNullOrEmpty(to) ? (DateTime?)null : DateTime.ParseExact(to, Constant.TIME_DATE_FORMAT_WITHOUT_SECOND, null);
            var visit = GetVisit(visit_id, visit_type);
            var query = (from ipd_sql in unitOfWork.EIOCareNoteRepository.AsQueryable().Where(
                         e => !e.IsDeleted && e.VisitId == visit_id && e.FormId == formId_newborn)
                         join doc_sql in unitOfWork.UserRepository.AsQueryable()
                            on ipd_sql.CreatedBy equals doc_sql.Username into dlist
                         join tab_newborn in unitOfWork.EIOConstraintNewbornAndPregnantWomanRepository.AsQueryable()
                         .Where(e => !e.IsDeleted)
                            on ipd_sql.FormId equals tab_newborn.Id into temp_infor
                         from doc_sql in dlist.DefaultIfEmpty()
                         from infor_tab_newborn in temp_infor.DefaultIfEmpty()
                         select new IPDCareNoteQueryModel
                         {
                             Id = ipd_sql.Id,
                             CareNote = ipd_sql.CareNote,
                             NoteTime = ipd_sql.NoteTime,
                             CreatedBy = ipd_sql.CreatedBy,
                             Fullname = doc_sql.Fullname,
                             VisitId = ipd_sql.VisitId,
                             ProgressNote = ipd_sql.ProgressNote,
                             FormId = infor_tab_newborn.Id,
                             Beb = infor_tab_newborn.Bed,
                             Room = infor_tab_newborn.Room,
                         });
            if (!string.IsNullOrEmpty(createdBy))
                query = query.Where(e => e.CreatedBy == createdBy);
            if (start != null && end != null)
                query = query.Where(e => e.NoteTime != null && e.NoteTime >= start && e.NoteTime <= end);
            else if (start != null)
                query = query.Where(e => e.NoteTime != null && e.NoteTime >= start);
            else if (end != null)
                query = query.Where(e => e.NoteTime != null && e.NoteTime <= end);

            query = query.GroupBy(e => e.Id).Select(e => e.FirstOrDefault());

            var result = from e in query
                         join c in unitOfWork.EIOFormConfirmRepository.AsQueryable()
                         .Where(e => !e.IsDeleted)
                         on e.Id equals c.FormId into temp
                         from res in temp.DefaultIfEmpty()
                         select new
                         {
                             e.Id,
                             NoteTime = e.NoteTime,
                             e.CreatedBy,
                             e.ProgressNote,
                             e.CareNote,
                             e.VisitId,
                             e.Fullname,
                             e.FormId,
                             e.Beb,
                             e.Room,
                             e.CreatedAt,
                             e.UpdatedAt,
                             e.UpdatedBy,
                             confirm = res
                         };

            if (sort == null || sort == 1)
                result = result.OrderByDescending(e => e.NoteTime);
            else
                result = result.OrderBy(e => e.NoteTime);
           
         

            return result.ToList().Select(e => new
            {
                e.Id,
                NoteTime = e.NoteTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                e.CreatedBy,
                e.ProgressNote,
                e.CareNote,
                e.VisitId,
                e.Fullname,
                Room =  e.FormId == null ? visit.Room : e.Room,
                Bed = e.FormId == null ? visit.Bed : e.Beb,
                IsLocked = visit_type == "IPD" ? IPDIsBlock((IPD)visit, Constant.IPDFormCode.PhieuChamSoc) : false,
                e.CreatedAt,
                e.UpdatedAt,
                e.UpdatedBy,
                ConfirmCreated = new
                {
                    e.confirm?.ConfirmAt,
                    e.confirm?.ConfirmBy,
                    e.confirm?.ConfirmType,
                    e.confirm?.Note,
                }
            }).ToList();
        }

        protected EIOCareNote GetEIOCareNote(Guid note_id)
        {
            return unitOfWork.EIOCareNoteRepository.GetById(note_id);
        }

        protected EIOCareNote CreateEIOCareNote(Guid visit_id, string visit_type, JObject request, Guid? formId = null)
        {
            var pNote = new EIOCareNote()
            {
                VisitId = visit_id,
                VisitTypeGroupCode = visit_type,
                ProgressNote = request["ProgressNote"]?.ToString(),
                CareNote = request["CareNote"]?.ToString(),
                FormId = formId
            };
            var room = request["Room"]?.ToString();
            var bed = request["Bed"]?.ToString();

            var visit = GetVisit(visit_id, visit_type);
            if (pNote.FormId == null)
            {
                visit.Room = room;
                visit.Bed = bed;
            }
            else
            {
                var tab_newborn = unitOfWork.EIOConstraintNewbornAndPregnantWomanRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == pNote.FormId);
                if(tab_newborn != null)
                {
                    tab_newborn.Room = room;
                    tab_newborn.Bed = bed;
                    unitOfWork.EIOConstraintNewbornAndPregnantWomanRepository.Update(tab_newborn);
                    unitOfWork.Commit();
                }    
            }    

            var time = request["NoteTime"]?.ToString();
            if (!string.IsNullOrEmpty(time))
                pNote.NoteTime = DateTime.ParseExact(time, Constant.TIME_DATE_FORMAT_WITHOUT_SECOND, null);
            else
                pNote.NoteTime = null;

            unitOfWork.EIOCareNoteRepository.Add(pNote);
            unitOfWork.Commit();
            return pNote;
        }

        protected EIOCareNote UpdateEIOCareNote(Guid note_id, JObject request, Guid? formId = null)
        {
            var dbItem = unitOfWork.EIOCareNoteRepository.GetById(note_id);
            dbItem.ProgressNote = request["ProgressNote"]?.ToString();
            dbItem.CareNote = request["CareNote"]?.ToString();
            var time = request["NoteTime"]?.ToString();
            if (!string.IsNullOrEmpty(time))
                dbItem.NoteTime = DateTime.ParseExact(time, Constant.TIME_DATE_FORMAT_WITHOUT_SECOND, null);
            else
                dbItem.NoteTime = null;
            var room = request["Room"]?.ToString();
            var bed = request["Bed"]?.ToString();
            var visit = GetVisit((Guid)dbItem.VisitId, dbItem.VisitTypeGroupCode);
            if (dbItem.FormId == null)
            {
                visit.Room = room;
                visit.Bed = bed;
            }
            else
            {
                var tab_newborn = unitOfWork.EIOConstraintNewbornAndPregnantWomanRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == dbItem.FormId);
                if (tab_newborn != null)
                {
                    tab_newborn.Room = room;
                    tab_newborn.Bed = bed;
                    unitOfWork.EIOConstraintNewbornAndPregnantWomanRepository.Update(tab_newborn);
                    unitOfWork.Commit();
                }
            }
            dbItem.FormId = formId;
            unitOfWork.EIOCareNoteRepository.Update(dbItem);
            unitOfWork.Commit();
            return dbItem;
        }
    }
}
