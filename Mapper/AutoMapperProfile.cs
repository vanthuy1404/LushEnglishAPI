// 19/11/2025 - 20:46:00
// DANGTHUY

using AutoMapper;
using LushEnglishAPI.Controllers;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Topic
        CreateMap<Topic, TopicDTO>();
        // Vocab
        CreateMap<Vocabulary, VocabularyDTO>();
        CreateMap<Practice, PracticeDTO>();
        CreateMap<Question, QuestionDTO>().ReverseMap();
        CreateMap<ChattingConfig, ChattingExerciseDTO>();
        CreateMap<WritingConfig, WritingExerciseDTO>();
        CreateMap<User, UserInfoDTO>();
        CreateMap<Result, UserResultDTO>();
        CreateMap<Course, CourseDTO>()
            .ForMember(dest => dest.Topics, opt => opt.Ignore())
            .ForMember(dest => dest.TopicCount, opt => opt.Ignore());
    }
}