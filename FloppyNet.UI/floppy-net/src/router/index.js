import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
    },
    {
      path: "/user/:id(\\d+)",
      name: "user",
      component: () => import("../views/UserView.vue"),
    },
    {
      path: "/history/:id(.*)*",
      name: "history",
      component: () => import("../views/HistoryView.vue"),
    },
  ],
});

export default router;
