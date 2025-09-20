using QBM.CompositionApi.Definition;
using VI.DB;
using VI.DB.Entities;

namespace QBM.CompositionApi
{
    public class CCCDenyAttestationAction : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("webportalplus/denyattestation/action")
                .Handle<PostedID>("POST", async (posted, qr, ct) =>
                {
                    var strUID_Person = qr.Session.User().Uid;
                    string uidcase = "";
                    bool Decision = true;
                    string Reason = null;
                    string UidJustification = null;
                    int SubLevel = -1;

                    foreach (var column in posted.columns)
                    {
                        if (column.column == "xAttKey")
                        {
                            uidcase = column.value;
                        }
                    }

                    string wc = String.Format("UID_AttestationCase = '{0}' and uid_personhead = '{1}'", uidcase, strUID_Person);
                    bool ex = await qr.Session.Source().ExistsAsync("ATT_VAttestationDecisionPerson", wc, ct).ConfigureAwait(false);
                    if (!ex)
                    {
                        throw new InvalidOperationException("You are not the eligible approver for this attestation case.");
                    }

                    var query1 = Query.From("AttestationCase").SelectAll().Where(String.Format("UID_AttestationCase = '{0}'", uidcase));
                    var tryget = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);

                    IEntity attestationCase = tryget.Result;

                    int num = SubLevel;
                    await attestationCase.CallMethodAsync("MakeDecision", new object[5]
                    {
                        qr.Session.User().Uid,
                        Decision,
                        Reason,
                        UidJustification,
                        num
                    }, ct).ConfigureAwait(continueOnCapturedContext: false);
                    await attestationCase.SaveAsync(qr.Session, ct).ConfigureAwait(continueOnCapturedContext: false);
                }));
        }
        public class PostedID
        {
            public columnsarray[] element { get; set; }
            public columnsarray[] columns { get; set; }
        }
        public class columnsarray
        {
            public string column { get; set; }
            public string value { get; set; }
        }
    }
}