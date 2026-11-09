# Suggested meaningful Part 3 GitHub commits

Make these as separate commits while implementing/reviewing the corresponding changes. Do not create empty commits; each commit should contain the named working change.

1. `Add RaceDay MVC project to solution and CI build`
2. `Create typed MVC view models for events and categories`
3. `Improve API client session token handling and errors`
4. `Store authenticated user ID in MVC session`
5. `Add organiser dashboard enrolment summaries`
6. `Implement organiser event editing workflow`
7. `Implement organiser event deletion workflow`
8. `Add event category creation and assignment pages`
9. `Add event category removal with API constraints`
10. `Add organiser event enrolment listing`
11. `Add organiser results entry and update forms`
12. `Return event dates and categories in participant results`
13. `Calculate finishing position against event finishers`
14. `Add participant profile update page`
15. `Add profile picture upload through API`
16. `Add upcoming event type and date filters`
17. `Display event banner and category details`
18. `Refresh RaceDay responsive Bootstrap design`
19. `Harden API image upload validation`
20. `Add multi-stage non-root MVC Docker image`
21. `Update GitHub Actions to build MVC and test API`
22. `Document Part 3 local Azure and Docker setup`

Example workflow:

```bash
git status
git add RaceDay.Mvc/Controllers/OrganizerController.cs
git commit -m "Implement organiser event editing workflow"
git push
```

Use the actual files changed in each commit and do not claim commits have been pushed until GitHub confirms them.
