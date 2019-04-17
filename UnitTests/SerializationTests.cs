using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class SerializationTests
    {
        private const string normalMessage = @"{'topic':'events',
                'ref':null,
                'payload':
                {
                    'timestamp':'2019-04-14T18:33:04.036625Z',
                    'status':'finished',
                    'machine_id':'68015cc1-3119-42d2-9d4e-3e824723fe03',
                    'id':'d525493b-7cbf-47c9-bdeb-1d07b9fd181c'
                },
                'join_ref':null,
                'event':'new'}";
        private const string customDataIntoStreamEvent = @"{'topic':'events',
            'ref':null,
            'payload':
            {
                'timestamp':'2019-04-14T18:33:04.036625Z',
                'status':'finished',
                'machine_id':'68015cc1-3119-42d2-9d4e-3e824723fe03',
                'id':'d525493b-7cbf-47c9-bdeb-1d07b9fd181c'
            },
            'join_ref':null,
            'event':'new',
            'custom_1':'custom_1',
            'custom_2':
            {
                'custom_prop_1':'custom_prop_1'
            },
            'custom_3':['custom_1','custom_2','custom_3'],
            'custom_4':100}";

        private const string customDataIntoPayload = @"{'topic':'events',
            'ref':null,
            'payload':
            {
                'timestamp':'2019-04-14T18:33:04.036625Z',
                'status':'finished',
                'machine_id':'68015cc1-3119-42d2-9d4e-3e824723fe03',
                'id':'d525493b-7cbf-47c9-bdeb-1d07b9fd181c',
                'custom_1':'custom_1',
                'custom_2':
                {
                    'custom_prop_1':'custom_prop_1'
                },
                'custom_3':['custom_1','custom_2','custom_3'],
                'custom_4':100
            },
            'join_ref':null,
            'event':'new'}";

        private const string customDataIntoStreamEventAndPayload = @"{'topic':'events',
            'ref':null,
            'payload':
            {
                'timestamp':'2019-04-14T18:33:04.036625Z',
                'status':'finished',
                'machine_id':'68015cc1-3119-42d2-9d4e-3e824723fe03',
                'id':'d525493b-7cbf-47c9-bdeb-1d07b9fd181c',
                'custom_1':'custom_1',
                'custom_2':
                {
                    'custom_prop_1':'custom_prop_1'
                },
                'custom_3':['custom_1','custom_2','custom_3'],
                'custom_4':100
            },
            'join_ref':null,
            'event':'new',
            'custom_1':'custom_1',
            'custom_2':
            {
                'custom_prop_1':'custom_prop_1'
            },
            'custom_3':['custom_1','custom_2','custom_3'],
            'custom_4':100}";

        [Theory]
        [InlineData(normalMessage)]
        [InlineData(customDataIntoStreamEvent)]
        [InlineData(customDataIntoPayload)]
        [InlineData(customDataIntoStreamEventAndPayload)]
        public void TypeSocketMessage_Test(string jsonLikeMessage)
        {
            var streamEvent = JsonConvert.DeserializeObject<StreamEvent>(jsonLikeMessage);

            Assert.IsType<StreamEvent>(streamEvent);
            Assert.IsAssignableFrom<IStreamEvent>(streamEvent);
            Assert.IsType<Payload>(streamEvent.Payload);
            Assert.IsAssignableFrom<IPayload>(streamEvent.Payload);

            Assert.IsType<string>(streamEvent.Topic);
            Assert.Null(streamEvent.Ref);
            Assert.Null(streamEvent.JoinRef);
            Assert.IsType<string>(streamEvent.Event);
            Assert.IsType<string>(streamEvent.Payload.Status);
            Assert.IsType<string>(streamEvent.Payload.Timestamp);
            Assert.IsType<string>(streamEvent.Payload.MachineId);
            Assert.IsType<string>(streamEvent.Payload.Id);
        }

        [Theory]
        [InlineData(normalMessage)]
        [InlineData(customDataIntoStreamEvent)]
        [InlineData(customDataIntoPayload)]
        [InlineData(customDataIntoStreamEventAndPayload)]
        public void ValueSocketMessage_Test(string jsonLikeMessage)
        {
            var streamEvent = JsonConvert.DeserializeObject<StreamEvent>(jsonLikeMessage);

            Assert.Equal("events", streamEvent.Topic);
            Assert.Equal("new", streamEvent.Event);
            Assert.Equal("finished", streamEvent.Payload.Status);
            Assert.Equal("2019-04-14T18:33:04.036625Z", streamEvent.Payload.Timestamp);
            Assert.Equal("68015cc1-3119-42d2-9d4e-3e824723fe03", streamEvent.Payload.MachineId);
            Assert.Equal("d525493b-7cbf-47c9-bdeb-1d07b9fd181c", streamEvent.Payload.Id);
        }

        [Theory]
        [InlineData(normalMessage)]
        [InlineData(customDataIntoStreamEvent)]
        [InlineData(customDataIntoPayload)]
        [InlineData(customDataIntoStreamEventAndPayload)]
        public void DataTypeSocketMessage_Test(string jsonLikeMessage)
        {
            var streamEvent = JsonConvert.DeserializeObject<StreamEvent>(jsonLikeMessage);

            Assert.Contains(streamEvent.Payload.Status, ModelHelpers.GetStreamEventStatuses());

            DateTime dateTime = DateTime.UtcNow;
            bool valid = false;
            valid = DateTime.TryParse(streamEvent.Payload.Timestamp, out dateTime);
            Assert.True(valid);

            Guid guid = Guid.Empty;
            valid = Guid.TryParse(streamEvent.Payload.MachineId, out guid);
            Assert.True(valid);

            valid = Guid.TryParse(streamEvent.Payload.Id, out guid);
            Assert.True(valid);
        }
    }
}
