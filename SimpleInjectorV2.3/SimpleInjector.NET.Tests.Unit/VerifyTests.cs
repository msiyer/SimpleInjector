﻿namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;

    [TestClass]
    public class VerifyTests
    {
        [TestMethod]
        public void Verify_WithEmptyConfiguration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_CalledMultipleTimes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            container.Verify();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_CalledAfterGetInstance_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            container.GetInstance<IUserRepository>();

            container.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "An exception was expected because the configuration is invalid without registering an IUserRepository.")]
        public void Verify_WithDependantTypeNotRegistered_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // RealUserService has a constructor that takes an IUserRepository.
            container.RegisterSingle<RealUserService>();

            // Act
            container.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Verify_WithFailingFunc_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<IUserRepository>(() =>
            {
                throw new ArgumentNullException();
            });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_RegisteredCollectionWithValidElements_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterAll<IUserRepository>(new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories = new IUserRepository[] { null };

            container.RegisterAll<IUserRepository>(repositories);

            try
            {
                // Act
                container.Verify();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.StringContains(
                    "One of the items in the collection for type IUserRepository is a null reference.",
                    ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Verify_FailingCollection_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories =
                from nullRepository in Enumerable.Repeat<IUserRepository>(null, 1)
                where nullRepository.ToString() == "This line fails with an NullReferenceException"
                select nullRepository;

            container.RegisterAll<IUserRepository>(repositories);

            // Act
            container.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Verify_RegisterCalledWithFuncReturningNullInstances_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository>(() => null);

            // Act
            container.Verify();
        }
        
        [TestMethod]
        public void Verify_GetRegistrationCalledOnUnregisteredAbstractType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This call forces the registration of a null reference to speed up performance.
            container.GetRegistration(typeof(IUserRepository));

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Register_WithAnOverrideCalledAfterACallToVerify_FailsWithTheExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.AllowOverridingRegistrations = true;

            container.Register<IUserRepository, SqlUserRepository>();
            
            container.Verify();

            try
            {
                // Act
                container.RegisterSingle<IUserRepository, SqlUserRepository>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void ResolveUnregisteredType_CalledAfterACallToVerify_FailsWithTheExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            try
            {
                // Act
                container.ResolveUnregisteredType += (s, e) => { };

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void ExpressionBuilding_CalledAfterACallToVerify_FailsWithTheExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            try
            {
                // Act
                container.ExpressionBuilding += (s, e) => { };

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void ExpressionBuilt_CalledAfterACallToVerify_FailsWithTheExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            try
            {
                // Act
                container.ExpressionBuilt += (s, e) => { };

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }
        
        [TestMethod]
        public void Verify_RegisterAllCalledWithUnregisteredType_ThrowsExpectedException()
        {
            // Arrange
            string expectedException = "No registration for type IUserRepository could be found.";

            var container = ContainerFactory.New();

            var types = new[] { typeof(SqlUserRepository), typeof(IUserRepository) };

            container.RegisterAll<IUserRepository>(types);

            try
            {
                // Act
                container.Verify();

                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                string actualMessage = ex.Message;

                string exceptionInfo = string.Empty;

                Exception exception = ex;

                while (exception != null)
                {
                    exceptionInfo +=
                        exception.GetType().FullName + Environment.NewLine +
                        exception.Message + Environment.NewLine +
                        exception.StackTrace + Environment.NewLine + Environment.NewLine;

                    exception = exception.InnerException;
                }

                AssertThat.StringContains(expectedException, actualMessage, "Info:\n" + exceptionInfo);
            }
        }

        [TestMethod]
        public void Verify_OnCollection_IteratesTheCollectionOnce()
        {
            // Arrange
            int expectedNumberOfCreatedPlugins = 1;
            int actualNumberOfCreatedPlugins = 0;

            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl));

            container.RegisterInitializer<PluginImpl>(plugin => actualNumberOfCreatedPlugins++);
            
            // Act
            container.Verify();

            // Assert
            Assert.AreEqual(expectedNumberOfCreatedPlugins, actualNumberOfCreatedPlugins);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Verify_CollectionWithDecoratorThatCanNotBeCreatedAtRuntime_ThrowsInvalidOperationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl));

            // FailingConstructorDecorator constructor throws an exception.
            container.RegisterDecorator(typeof(IPlugin), typeof(FailingConstructorPluginDecorator));

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_WithCollectionsResolvedThroughUnregisteredTypeResolution_StillVerifiesThoseCollections()
        {
            // Arrange
            // All these collection are resolved through unregistered type resolution.
            var expectedTypes = new[]
            {
                typeof(IEnumerable<Service<Service<Service<int>>>>),
                typeof(IEnumerable<Service<Service<int>>>),
                typeof(IEnumerable<Service<int>>),
                typeof(IEnumerable<int>),
            };

            var container = ContainerFactory.New();

            container.RegisterAllOpenGeneric(typeof(Service<>), typeof(Service<>));

            container.Register<Service<Service<Service<Service<int>>>>>();

            container.Verify();

            // Act
            var registrations = container.GetCurrentRegistrations();
            var actualTypes = registrations.Select(p => p.ServiceType).ToList();

            // Assert
            var missingTypes = expectedTypes.Where(registration => !actualTypes.Contains(registration));

            // When the missingTypes list is empty, this means that the container kept looking for new 
            // registrations at the end of the verification process.
            Assert.IsFalse(missingTypes.Any(), "Missing registrations: " + missingTypes.ToFriendlyNamesText());
        }

        public class Service<T>
        {
            public Service(IEnumerable<T> collection)
            {
            }
        }
        
        private sealed class PluginDecorator : IPlugin
        {
            public PluginDecorator(IPlugin plugin)
            {
            }
        }

        private sealed class FailingConstructorPluginDecorator : IPlugin
        {
            public FailingConstructorPluginDecorator(IPlugin plugin)
            {
                throw new NotImplementedException();
            }
        }
    }
}