using Improbable;
using Improbable.Context;
using Improbable.Worker;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class ComponentMapTest
    {
        private ComponentMapTestWrapper<Position> _componentMap;

        [SetUp]
        public void Setup()
        {
            _componentMap = new ComponentMapTestWrapper<Position>(new Mock<IDispatcher>().Object);
        }

        [Test]
        public void TestComponentMapSetup()
        {
            Assert.AreEqual(0, _componentMap.Keys.Count);
            Assert.False(_componentMap.HasUpdated());
        }
        
        [Test]
        public void TestAddNewComponent()
        {
            AddComponentOp(new EntityId(1), new Position.Data(new Coordinates(1, 2, 3)));
            
            Assert.AreEqual(1, _componentMap.Keys.Count);
        }

        [Test]
        public void TestGetComponent()
        {
            var entityId = new EntityId(1);
            var data = new Position.Data(new Coordinates(1, 2, 3));    
            AddComponentOp(entityId, data);
            
            Assert.AreEqual(data, _componentMap.Get(entityId));
        }

        [Test]
        public void TestAddComponentWithExistingEntityId()
        {
            var entityId = new EntityId(1);
            var existingData = new Position.Data(new Coordinates(1, 2, 3));
            var newData = new Position.Data(new Coordinates(4, 5, 6));
            
            AddComponentOp(entityId, existingData);
            AddComponentOp(entityId, newData);
            
            Assert.AreEqual(newData, _componentMap.Get(entityId));
            Assert.AreEqual(1, _componentMap.Keys.Count);
        }
        
        [Test]
        public void TestHasAuthorityIsFalse()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            Assert.False(_componentMap.HasAuthority(entityId));
        }

        [Test]
        public void TestHasAuthorityLossImminentIsFalse()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            Assert.False(_componentMap.HasAuthorityLossImminent(entityId));
        }

        [Test]
        public void TestAuthoritativeChangeOpToAuthoritative()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));

            SetAuthority(entityId, Authority.Authoritative);
            
            Assert.True(_componentMap.HasAuthority(entityId));
            Assert.False(_componentMap.HasAuthorityLossImminent(entityId));
        }

        [Test]
        public void TestAuthoritativeChangeOpToNotAuthoritative()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            SetAuthority(entityId, Authority.NotAuthoritative);
            
            Assert.False(_componentMap.HasAuthority(entityId));
            Assert.False(_componentMap.HasAuthorityLossImminent(entityId));
        }
        
        [Test]
        public void TestAuthoritativeChangeOpToAuthorityLossImminent()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            SetAuthority(entityId, Authority.AuthorityLossImminent);
            
            Assert.False(_componentMap.HasAuthority(entityId));
            Assert.True(_componentMap.HasAuthorityLossImminent(entityId));
        }

        [Test]
        public void TestContainsKey()
        {
            var entityId = new EntityId(1);
            
            Assert.False(_componentMap.ContainsKey(entityId));
            
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            Assert.True(_componentMap.ContainsKey(entityId));
        }

        [Test]
        public void TestUpdatComponentThatDoesNotExist()
        {
            var entityId = new EntityId(1);
            var positionUpdate = new Position.Update { coords = new Coordinates(4, 5, 6) };

            UpdateComponent(entityId, positionUpdate);
            
            Assert.False(_componentMap.ContainsKey(entityId));
        }

        [Test]
        public void TestUpdateComponentWithoutAuthority()
        {
            var entityId = new EntityId(1);
            var data = new Position.Data(new Coordinates(1, 2, 3));    
            AddComponentOp(entityId, data);
            
            var positionUpdate = new Position.Update { coords = new Coordinates(4, 5, 6) };

            UpdateComponent(entityId, positionUpdate);
            
            Assert.AreNotEqual(data.Value.coords, positionUpdate.coords);
        }

        [Test]
        public void TestUpdateExistingComponent()
        {
            var entityId = new EntityId(1);
            var data = new Position.Data(new Coordinates(1, 2, 3));    
            AddComponentOp(entityId, data);
            SetAuthority(entityId, Authority.Authoritative);
            
            var positionUpdate = new Position.Update { coords = new Coordinates(4, 5, 6) };

            UpdateComponent(entityId, positionUpdate);
            
            Assert.AreEqual(data.Value.coords, positionUpdate.coords);
        }

        [Test]
        public void TestRemoveEntityThatDoesNotExist()
        {
            var entityId = new EntityId(1);
            RemoveEntity(entityId);
            
            Assert.AreEqual(0, _componentMap.Keys.Count);
        }

        [Test]
        public void TestRemoveExistingEntity()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            SetAuthority(entityId, Authority.Authoritative);
            RemoveEntity(entityId);
            
            Assert.False(_componentMap.ContainsKey(entityId));
            Assert.False(_componentMap.HasAuthority(entityId));
            Assert.False(_componentMap.HasAuthorityLossImminent(entityId));
        }

        [Test]
        public void TestTryGetValueIsFalse()
        {
            Assert.False(_componentMap.TryGetValue(new EntityId(1), out var component));
            Assert.Null(component);
        }

        [Test]
        public void TestTryGetValueIsTrue()
        {
            var entityId = new EntityId(1);
            var data = new Position.Data(new Coordinates(1, 2, 3));    
            AddComponentOp(entityId, data);
            
            Assert.True(_componentMap.TryGetValue(new EntityId(1), out var component));
            Assert.AreEqual(data.Value, component.Get().Value);
        }

        [Test]
        public void TestHasUpdatedOnAdd()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            Assert.True(_componentMap.HasUpdated());
        }
        
        [Test]
        public void TestAckUpdate()
        {
            var entityId = new EntityId(1);
            AddComponentOp(entityId, new Position.Data(new Coordinates(1, 2, 3)));
            
            _componentMap.AckUpdated();
            
            Assert.False(_componentMap.HasUpdated());
        }

        [Test]
        public void TestHasUpdatedOnRemove()
        {
            var entityId = new EntityId(1);
            var data = new Position.Data(new Coordinates(1, 2, 3));    
            AddComponentOp(entityId, data);
            
            _componentMap.AckUpdated();
            
            RemoveEntity(entityId);
            
            Assert.True(_componentMap.HasUpdated());
        }

        [Test]
        public void TestHasUpdatedOnComponentUpdate()
        {
            var entityId = new EntityId(1);
            var data = new Position.Data(new Coordinates(1, 2, 3));    
            AddComponentOp(entityId, data);
            SetAuthority(entityId, Authority.Authoritative);
            
            var positionUpdate = new Position.Update { coords = new Coordinates(4, 5, 6) };
            
            _componentMap.AckUpdated();

            UpdateComponent(entityId, positionUpdate);
            
            Assert.True(_componentMap.HasUpdated());
        }

        private void AddComponentOp(EntityId entityId, Position.Data data)
        {
            var addComponentOp = new AddComponentOp<Position>
            {
                EntityId = entityId, 
                Data = data
            };
            
            _componentMap.AddComponent(addComponentOp);
        }

        private void SetAuthority(EntityId entityId, Authority authority)
        {
            var authorityChangeOp = new AuthorityChangeOp
            {
                EntityId = entityId,
                Authority = authority
            };
            
            _componentMap.SetAuthority(authorityChangeOp);
        }

        private void UpdateComponent(EntityId entityId, Position.Update update)
        {
            var componentUpdateOps = new ComponentUpdateOp<Position>
            {
                EntityId = entityId,
                Update = update
            };

            _componentMap.UpdateComponent(componentUpdateOps);
        }

        private void RemoveEntity(EntityId entityId)
        {
            var removeEntityOp = new RemoveEntityOp { EntityId = entityId };

            _componentMap.RemoveEntity(removeEntityOp);
        }
    }
}