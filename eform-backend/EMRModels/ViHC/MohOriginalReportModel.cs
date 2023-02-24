using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRModels.ViHC
{
    class MohOriginalReportModel
    {
        #region Hospital location - Thông tin về bệnh viện
        /// <summary>
        /// Tên cơ sở (Site name), ex: Vinmec TimeCity
        /// </summary>
        public string SystemHospitalName { get; set; }
        ///Mã viết tắt cơ sở (Site code), ex: HTC
        /// </summary>
        public string SystemHospitalCode { get; set; }
        #endregion
        public string CurrentDateFormat { get; set; }
        public string CurrentDateTimeFormat { get; set; }
        public string ReportOrderNumber { get; set; }
        #region THÔNG TIN BỆNH NHÂN
        public string Name { get; set; }
        public string PID { get; set; }
        public DateTime DOB { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
        /// <summary>
        ///Quốc gia
        /// </summary>
        public string Nationality { get; set; }
        /// <summary>
        ///Số CMND hoặc hộ chiếu
        /// </summary>
        public string IdCardOrPassport { get; set; }
        /// <summary>
        ///Ngày cấp CMND hoặc hộ chiếu
        /// </summary>
        public string DateOfissueOfIdCardOrPassport { get; set; }
        /// <summary>
        ///Nơi cấp CMND hoặc hộ chiếu
        /// </summary>
        public string PlaceOfissueOfIdCardOrPassport { get; set; }
        public string DOBString
        {
            get { return DOB.ToShortDateString(); }
        }

        public int? Age
        {
            get
            {
                if (DOB == null || DOB.Year <= 1900)
                    return 0;
                else
                    return DateTime.Now.Year - DOB.Year;
            }
        }

        public string Sex { get; set; } = "M";

        public string ExaminationNumber { get; set; }
        public string ExaminationCourse { get; set; }
        public string DoctorExaminationBy { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string ExaminationDateString
        {
            get { return ExaminationDate.ToShortDateString(); }
        }
        /// <summary>
        /// Ngày kết luận = Ngày Bs GP click hoàn thành lần đầu
        /// </summary>
        public DateTime ConclusionGPDate { get; set; }
        /// <summary>
        ///Lý do khám sức khỏe
        /// </summary>
        public string Reasonforhealthcheck { get; set; }
        #endregion
        /// <summary>
        ///Phân loại sức khỏe Tổng thể
        /// </summary>
        public string ClassifyHealthy { get; set; }
        /// <summary>
        /// Phân loại tùy chọn bởi GP
        /// </summary>
        public bool ClassifyCustomGP { get; set; }
        #region PHÂN LOẠI SỨC KHỎE THEO CHUYÊN KHOA
        /// <summary>
        ///Phân loại Khám thể lực
        /// </summary>
        public string ClassifyAnthropometry { get; set; }
        /// <summary>
        ///Phân loại chuyên khoa Tai-Mũi-Họng
        /// </summary>
        public string ClassifyENT { get; set; }
        /// <summary>
        /// Chuyên khoa Mắt
        /// </summary>
        public string ClassifyEye { get; set; }
        /// <summary>
        /// CHuyên khoa răng - Hàm - mặt
        /// </summary>
        public string ClassifyDentistry { get; set; }
        /// <summary>
        ///Hệ cơ xương khớp (Hệ Vận động)
        /// </summary>
        public string ClassifyMusculoskeletal { get; set; }
        /// <summary>
        ///Thần kinh
        /// </summary>
        public string ClassifyNeurology { get; set; }
        /// <summary>
        ///Tâm thần
        /// </summary>
        public string ClassifyMental { get; set; }
        /// <summary>
        ///Da Liễu
        /// </summary>
        public string ClassifyDermatology { get; set; }
        /// <summary>
        ///Hệ tuần hoàn (Tim mạch)
        /// </summary>
        public string ClassifyCirculatory { get; set; }
        /// <summary>
        ///Hô hấp
        /// </summary>
        public string ClassifyRespiratory { get; set; }
        /// <summary>
        ///Thận-Tiết niệu
        /// </summary>
        public string ClassifyKidneyUrology { get; set; }
        /// <summary>
        ///Hệ tiêu hóa
        /// </summary>
        public string ClassifyGastroenterology { get; set; }
        /// <summary>
        ///Ngoại khoa
        /// </summary>
        public string ClassifySurgery { get; set; }
        /// <summary>
        ///Sản Phụ Khoa
        /// </summary>
        public string ClassifyGynecologyy { get; set; }
        #endregion
        #region TIỀN SỬ BỆNH ÁN (Áp dụng cho báo cáo MOH)
        /// <summary>
        /// Đánh dấu tiền sử gia đình có mắc 1 số bệnh được liệt kê trong Questionaire hay không
        /// </summary>
        public bool TienSuGiaDinhCoBenh { get; set; }
        /// <summary>
        /// Tiền sử gia đình
        /// </summary>
        public string TienSuGiaDinh { get; set; }
        /// <summary>
        /// Đánh dấu tiền sử bản thân có mắc 1 số bệnh được liệt kê trong Questionaire hay không
        /// </summary>
        public bool TienSuBanThanCoBenh { get; set; }
        /// <summary>
        /// Tiền sử bản thân
        /// </summary>
        public string TienSuBanThan { get; set; }
        /// <summary>
        /// Bệnh Nhân có đang điều trị bệnh? Nếu đang điều trị thì liệt kê các thuốc đang dùng + liều lượng
        /// </summary>
        public string DangSudungThuoc_LieuLuong { get; set; }
        /// <summary>
        /// Tien su thai sản
        /// </summary>
        public string TienSuThaiSan { get; set; }

        #endregion
        #region MEDICAL HISTORY / TIỀN SỬ Y KHOA (Áp dụng cho báo cáo C.O.E)
        /// <summary>
        /// Tien su noi khoa
        /// </summary>
        public string PastMedicalHistory { get; set; }
        /// <summary>
        /// Tien su ngoai khoa
        /// </summary>
        public string PastSurgicalHistory { get; set; }
        /// <summary>
        /// Di ung thuoc
        /// </summary>
        public string DrugAllergy { get; set; }
        /// <summary>
        /// Thuoc dang dung
        /// </summary>
        public string CurrentMedication { get; set; }
        /// <summary>
        /// Tien su san khoa
        /// </summary>
        public string ObstetricsHistory { get; set; }
        /// <summary>
        /// Tien su phu khoa
        /// </summary>
        public string GynaecologicalHistory { get; set; }
        /// <summary>
        /// Tiền sử gia đình
        /// </summary>
        public string SignificantFamilyHistory { get; set; }
        #endregion
        #region LIFE STYLE HISTORY / THÓI QUEN (Áp dụng cho báo cáo C.O.E)
        /// <summary>
        /// Thuốc lá
        /// </summary>
        public string LifeStyleHisSmoking { get; set; }
        /// <summary>
        /// Vận động
        /// </summary>
        public string LifeStyleHisExercise { get; set; }
        /// <summary>
        /// Rượu
        /// </summary>
        public string LifeStyleHisAlcohol { get; set; }
        /// <summary>
        /// Coffee
        /// </summary>
        public string LifeStyleHisCoffee { get; set; }

        #endregion
        #region I. KHÁM THỂ LỰC / Physical Measurement
        /// <summary>
        ///Chiều Cao
        /// </summary>
        public double? PhysicalHeightResult { get; set; }
        /// <summary>
        ///Cân nặng
        /// </summary>
        public double? PhysicalWeightResult { get; set; }
        /// <summary>
        ///Vong nguc
        /// </summary>
        public double? ChestValue { get; set; }
        /// <summary>
        /// Vong bung
        /// </summary>
        public double? PhysicalWaistCircumResult { get; set; }
        /// <summary>
        ///Chỉ số BMI
        /// </summary>
        public double? PhysicalBmiResult { get; set; }
        /// <summary>
        ///Mạch (Số lần / phút)
        /// </summary>
        public string CircuitResult { get; set; }
        /// <summary>
        ///huyết áp tối đa (tâm thu)
        /// </summary>
        public string BloodPressureMax { get; set; }
        /// <summary>
        ///huyết áp tối thiểu (tâm trương)
        /// </summary>
        public int? BloodPressureMin { get; set; }
        /// <summary>
        /// Nhịp thở
        /// </summary>
        public string BreathingResult { get; set; }
        #endregion
        #region II. KHÁM LÂM SÀNG / CLINICAL EXAMINATION
        #region NỘI KHOA / INTERNAL EXAMINATION
        /// <summary>
        ///Kết quả khám Tuần hoàn
        /// </summary>
        public string CirculatoryExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Hô hấp
        /// </summary>
        public string RespiratoryExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Tiêu hóa
        /// </summary>
        public string GastroenterologyExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Thận-Tiết niệu
        /// </summary>
        public string KidneyUrologyExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Cơ-xương-khớp
        /// </summary>
        public string MusculoskeletalExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Thần kinh
        /// </summary>
        public string NeurologyExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Tâm thần
        /// </summary>
        public string MentalExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Ngoại khoa
        /// </summary>
        public string SurgeryExamResult { get; set; }
        #region SPK
        /// <summary>
        ///Kết quả khám Sản phụ khoa
        /// </summary>
        public bool GynecologyIsShow { get; set; }
        /// <summary>
        ///SPK: Tiền sử
        /// </summary>
        public string GynecologyHisOfIllness { get; set; }
        /// <summary>
        /// SPK: Kết luân (Hoặc: Kết luân & Tư vấn)
        /// </summary>
        public string GynecologyExamResult { get; set; }
        /// <summary>
        /// SPK: Khám lâm sàng
        /// </summary>
        public string GynecologyClinicalResult { get; set; }
        /// <summary>
        /// SPK: Tư vấn
        /// </summary>
        public string GynecologyConsult { get; set; }
        #endregion .\SPK
        /// <summary>
        ///Kết quả khám Da liễu
        /// </summary>
        public bool DermatologyIsShow { get; set; }
        public string DermatologyExamResult { get; set; }

        #region B. KHÁM CHUYÊN KHOA / SPECLALLTY EXAMINATION
        #region MẮT / EYES
        /// <summary>
        ///Không kính: Mắt Phải / Right eye
        /// </summary>
        public string EyeRightWithoutGlassResult { get; set; }
        /// <summary>
        ///Không kính: Mắt trái / Left eye
        /// </summary>
        public string EyeLeftWithoutGlassResult { get; set; }
        /// <summary>
        ///Có kính: Mắt Phải / Right eye
        /// </summary>
        public string EyeRightWithGlassResult { get; set; }
        /// <summary>
        ///Có kính: Mắt trái / Left eye
        /// </summary>
        public string EyeLeftWithGlassResult { get; set; }
        /// <summary>
        ///Các bệnh về mắt khác (Nếu có)
        /// </summary>
        public string EyeDiseasesResult { get; set; }
        /// <summary>
        /// Mắt: Khám lâm sàng
        /// </summary>
        public string EyeClinicalResult { get; set; }
        /// <summary>
        /// Mắt: Tư vấn
        /// </summary>
        public string EyeConsult { get; set; }
        #endregion
        #region TAI-MŨI-HỌNG / E.N.T
        /// <summary>
        ///Nói thường: Tai trái
        /// </summary>
        public string EarLeftSpeakNormalExamResult { get; set; }
        /// <summary>
        ///Nói thường: Tai phải
        /// </summary>
        public string EarRightSpeakNormalExamResult { get; set; }
        /// <summary>
        ///Nói thầm: Tai trái
        /// </summary>
        public string EarLeftwhisperExamResult { get; set; }
        /// <summary>
        ///Nói thầm: Tai phải
        /// </summary>
        public string EarRightwhisperExamResult { get; set; }
        /// <summary>
        ///Các bệnh về tai mũi họng (nếu có)
        /// </summary>
        public string ENTdiseasesExamResult { get; set; }
        /// <summary>
        /// TMH: Khám lâm sàng
        /// </summary>
        public string ENTClinicalResult { get; set; }
        /// <summary>
        /// TMH: Tư vấn
        /// </summary>
        public string ENTConsult { get; set; }
        #endregion
        #region RĂNG-HÀM-MẶT
        /// <summary>
        ///Kết quả khám Răng hàm trên
        /// </summary>
        public string UpperJawExamResult { get; set; }
        /// <summary>
        ///Kết quả khám Răng hàm dưới
        /// </summary>
        public string LowerJawExamResult { get; set; }
        /// <summary>
        ///Các bệnh về Răng-Hàm-Mặt (nếu có)
        /// </summary>
        public string TeethJawFaceDiseasesExamResult { get; set; }
        /// <summary>
        /// RHM: Khám lâm sàng
        /// </summary>
        public string TeethJawFaceClinicalResult { get; set; }
        /// <summary>
        /// RHM: Tư vấn
        /// </summary>
        public string TeethJawFaceConsult { get; set; }
        #endregion
        #endregion
        #endregion
        #endregion
        #region CẬN LÂM SÀNG/LAB TESTS (paraclinical)
        #region Công thức máu / CBC
        /// <summary>
        ///Số lượng Hồng cầu
        /// </summary>
        public string SubCliQuantityRBCResult { get; set; }
        /// <summary>
        ///Số lượng Bạch cầu
        /// </summary>
        public string SubCliQuantityWBCResult { get; set; }
        /// <summary>
        ///Số lượng Tiểu cầu
        /// </summary>
        public string SubCliQuantityPlateletResult { get; set; }
        #endregion
        #region Sinh hóa máu
        /// <summary>
        ///Kết quả xét nghiệm lượng đường trong máu
        /// </summary>
        public string SubCliGlucoseResult { get; set; }
        /// <summary>
        ///Kết quả xét nghiệm lượng Urê (Máu)
        /// </summary>
        public string SubCliUreaResult { get; set; }
        /// <summary>
        ///Kết quả xét nghiệm lượng Creatinin (Máu)
        /// </summary>
        public string SubCliCreatininResult { get; set; }
        /// <summary>
        ///ASAT (GOT)
        /// </summary>
        public string SubCliAsatResult { get; set; }
        /// <summary>
        ///ALAT (GPT)
        /// </summary>
        public string SubCliAlatResult { get; set; }
        /// <summary>
        ///Các xét nghiệm khác liên quan đến bệnh về máu
        /// </summary>
        public string SubCliBloodDiseasesResult { get; set; }
        #endregion
        #region Xét nghiệm Nước Tiểu / urinalysis
        /// <summary>
        ///Lượng đường trong nước tiểu/ Glucose
        /// </summary>
        public string SubCliGlucoseInUrinalysisResult { get; set; }
        /// <summary>
        ///Protein trong nước tiểu
        /// </summary>
        public string SubCliProteinInUrinalysisResult { get; set; }
        /// <summary>
        ///Các xét nghiệm khác liên quan đến Nước tiểu
        /// </summary>
        public string SubCliUrinalysisDiseasesOtherResult { get; set; }
        #endregion
        #region Chuẩn Đoán Hình Ảnh / Image Examination
        public string ImageExamResult { get; set; }
        #endregion
        #endregion
        #region CHỮ KÝ BẰNG HÌNH ẢNH & HỌ TÊN CỦA BÁC SĨ CHUYÊN KHOA
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Tim mạch - Tuần hoàn
        /// </summary>
        public string ImgCardiologistSignature { get; set; }
        public string HoTenBSTimMach { get; set; }
        public string TitleBSTimMach { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Hô Hấp
        /// </summary>
        public string ImgRespiratoryDoctorSignature { get; set; }
        public string HoTenBSHoHap { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Tiêu Hóa
        /// </summary>
        public string ImgGastroenterologistSignature { get; set; }
        public string HoTenBSTieuHoa { get; set; }
        public string TitleBSTieuHoa { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Chuyên khoa Thận - Tiết Niệu
        /// </summary>
        public string ImgNephrologistSignature { get; set; }
        public string HoTenBSThanTietNieu { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Chuyên khoa Cơ-xương-khớp
        /// </summary>
        public string ImgMusculoskeletalDoctorSignature { get; set; }
        public string HoTenBSCoXuongKhop { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Chuyên Thần Kinh
        /// </summary>
        public string ImgNeurosurgeonSignature { get; set; }
        public string HoTenBSThanKinh { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Chuyên Tâm Thần
        /// </summary>
        public string ImgPsychiatristSignature { get; set; }
        public string HoTenBSTamThan { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Ngoại khoa
        /// </summary>
        public string ImgSurgerySignature { get; set; }
        public string TitleBSNgoaiKhoa { get; set; }
        public string HoTenBSNgoaiKhoa { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Sản
        /// </summary>
        public string ImgObstetricianSignature { get; set; }
        public string HoTenBSSan { get; set; }
        public string TitleBSSan { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Mắt
        /// </summary>
        public string ImgOphthalmologistSignature { get; set; }
        public string HoTenBSMat { get; set; }
        public string TitleBSMat { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Tai - Mũi - Họng
        /// </summary>
        public string ImgOtolaryngologistSignature { get; set; }
        public string HoTenBSTaiMuiHong { get; set; }
        public string TitleBSTaiMuiHong { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Răng - Hàm - Mặt
        /// </summary>
        public string ImgDentistSignature { get; set; }
        public string HoTenBSRangHamMat { get; set; }
        public string TitleBSRangHamMat { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Da liễu
        /// </summary>
        public string ImgDermatologistSignature { get; set; }
        public string HoTenBSDaLieu { get; set; }
        public string TitleBSDaLieu { get; set; }
        /// <summary>
        /// Hình ảnh chữ ký Bác sĩ Chẩn đoán hình ảnh
        /// </summary>
        public string ImgImageExamDoctorSignature { get; set; }
        public string HoTenBSChuanDoanHinhAnh { get; set; }
        #endregion
        #region Thong tin & Ket Luan cua BS GP
        public string DoctorGPSummaryContentResult { get; set; }
        public string DoctorGPSummaryContentSub { get; set; }
        public string ImageGPSignature { get; set; }
        public string HoTenBSGP { get; set; }
        public string TitleBSGP { get; set; }
        //linhht thêm Gp cận lâm sàng khác
        public string ImageGPCLSKSignature { get; set; }
        public string HoTenBSGPCLSK { get; set; }
        public string TitleBSGPCLSK { get; set; }
        #endregion Thong tin & Ket Luan cua BS GP

        public bool NoiTietIsShow { get; set; }
        public string NoiTietResult { get; set; }
        public string NoiTietClassify { get; set; }
        public string ImageNoiTiet { get; set; }
        public string HoTenBSNoiTiet { get; set; }
        /// <summary>
        /// Danh sách kết quả, đánh giá, kết luận theo chuyên khoa khác
        /// </summary>

        public bool IsCDHATyped { get; set; }
        public string TitleBSTKXN { get; set; }
        public string HoTenBSTKXN { get; set; }
        public string ChuKyBSTKXN { get; set; }
        public string TitleBSTKCDHA { get; set; }
        public string HoTenBSTKCDHA { get; set; }
        public string ChuKyBSTKCDHA { get; set; }
        public string MPNhanBietSangToi { get; set; }
        public string MTNhanBietSangToi { get; set; }
        public string MPBongBanTay { get; set; }
        public string MTBongBanTay { get; set; }
        public string MPDemNgonTay { get; set; }
        public string MTDemNgonTay { get; set; }
        public string MPKinhLo { get; set; }
        public string MTKinhLo { get; set; }
        //linhht
        

    }
}
