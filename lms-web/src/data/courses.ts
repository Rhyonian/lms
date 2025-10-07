export type Course = {
  id: string;
  title: string;
  category: string;
  duration: string;
  level: "Beginner" | "Intermediate" | "Advanced";
  description: string;
  instructor: string;
  heroVideoUrl: string;
  lessons: { id: string; title: string; videoUrl: string }[];
};

export const courses: Course[] = [
  {
    id: "CRS-101",
    title: "Designing Modern Learning Journeys",
    category: "Instructional Design",
    duration: "2h 30m",
    level: "Intermediate",
    instructor: "Priya Desai",
    description:
      "Learn how to create adaptive learning journeys that balance instructor-led and self-paced experiences for enterprise teams.",
    heroVideoUrl: "https://player.vimeo.com/video/76979871?h=dc1e1c7a8e",
    lessons: [
      {
        id: "CRS-101-1",
        title: "Learning Journey Foundations",
        videoUrl: "https://www.youtube.com/embed/dQw4w9WgXcQ"
      },
      {
        id: "CRS-101-2",
        title: "Designing for Engagement",
        videoUrl: "https://www.youtube.com/embed/rfscVS0vtbw"
      }
    ]
  },
  {
    id: "CRS-102",
    title: "Measuring Training Impact",
    category: "Analytics",
    duration: "1h 45m",
    level: "Beginner",
    instructor: "Alex Johnson",
    description:
      "Discover frameworks and tools for demonstrating the business impact of learning initiatives using both qualitative and quantitative data.",
    heroVideoUrl: "https://player.vimeo.com/video/137857207?h=8b6b6b1d3e",
    lessons: [
      {
        id: "CRS-102-1",
        title: "Data Foundations",
        videoUrl: "https://www.youtube.com/embed/2ePf9rue1Ao"
      },
      {
        id: "CRS-102-2",
        title: "Storytelling with Insights",
        videoUrl: "https://www.youtube.com/embed/Ke90Tje7VS0"
      }
    ]
  },
  {
    id: "CRS-103",
    title: "Facilitation Masterclass",
    category: "Leadership",
    duration: "3h 15m",
    level: "Advanced",
    instructor: "Jordan Smith",
    description:
      "A hands-on guide to facilitating transformational workshops, with templates and rituals you can bring to your next session.",
    heroVideoUrl: "https://player.vimeo.com/video/1084537?h=2f7c2c3b0e",
    lessons: [
      {
        id: "CRS-103-1",
        title: "Preparing the Space",
        videoUrl: "https://www.youtube.com/embed/1Rs2ND1ryYc"
      },
      {
        id: "CRS-103-2",
        title: "Leading with Presence",
        videoUrl: "https://www.youtube.com/embed/oHg5SJYRHA0"
      }
    ]
  }
];
