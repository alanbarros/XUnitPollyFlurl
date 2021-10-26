using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace TestProjet
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var configuration = new MapperConfiguration(cfg => {
                //cfg.AddExpressionMapping();
                cfg.AddProfile<OrganizationProfile>();
            });

            var mapper = configuration.CreateMapper();

            var classeB = new ClasseB
            {
                Data = DateTime.Now,
                Idade = 2,
                Nome = "Teste"
            };

            var listClasseA = new List<ClasseA>
            {
                mapper.Map<ClasseB, ClasseA>(classeB),
                new ClasseA
                {
                    Data = DateTime.Now,
                    Idade = 2,
                    Nome = "Maria"
                }
            };

            Expression<Func<ClasseB, bool>> dtoExpression = dto => dto.Nome.StartsWith("T");

            var expression = mapper.Map<Expression<Func<ClasseA, bool>>>(dtoExpression);

            Assert.NotEmpty(listClasseA);

        }
    }

    public class ClasseA
    {
        public string Nome { get; set; }
        public int Idade { get; set; }
        public DateTime Data { get; set; }
    }

    public class ClasseB
    {
        public string Nome { get; set; }
        public int Idade { get; set; }
        public DateTime Data { get; set; }
    }

    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<ClasseA, ClasseB>().ReverseMap();
        }
    }
}
