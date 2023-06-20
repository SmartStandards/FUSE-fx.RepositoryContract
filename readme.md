# FUSE-fx.RepositoryContract



Defines an **Service Interface** containing a set of functions to **access a data-store / repository** including CRUD operations, filtering and meta data-access to retrieve schema information.

This is designed to be implementable also as web service endpoint using the call-based approach of UJMW instead of rest (because we want more flexibility than HTTP-verbs can offer).

In addition to that you will find the following artefact's here:

```c#
ICrudAccess
IFlatView
IRepository //(inherits ICrudAccess + IFlatView)

EntityRef //replaces Navigation-Properties

SimpleExpressionTree //light expression tree for filtering, which is serializable
                    //(LINQ-Expressions aren't!)
                    
```



## History

After several experiments from [KornSW](https://github.com/KornSW/) in the past (see the ["kPers"-Project](https://github.com/KornSW/kPers/blob/master/kPers/kPers/IRepository.cs) containing the **IRepository** and the ["DynamicRepository"-Project](https://github.com/KornSW/DynamicRepository) which uses **LinqDynamic**) all the Ideas have been submitted to the 'SmartStandards' work group bringing all peaces together to find a really generic repo pattern that also supports multi-tier...