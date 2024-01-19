**This is the implementation of coding challenge test task on hiring to Sportradar company**

There are two versions of score dashboard functionality in this solution. <br/>
The first one came up to my head on reading the test task requirement.<br/>
The second one is as per test task requirements. <br/>
The first version is based on the next business logic: <br/>
1. The footboal matches can go through different stages <br/>
  - match is  annonced <br/>
  - match is started <br/>
  - match is finished <br/>
2. The summary about active matches only (started but not finished) should come to score dashboard. <br/>
3. Once match is started it's summary should appear on the score dashboard. <br/>
4. Once match is finished it's summary should vanish from the score dashboard. <br/>
5. During the match players can score a goals.<br/>
6. The summary order on the score dashboard is as per test task requirements.<br/>

I've decided to decouple this business logic on two interfaces:<br/>
- match dispatcher which is responsible for managing the matches (annonce, start, finish, record the goals)<br/>
- score dashboard data provider which is responsible for generating summaries about active matches<br/>
