const API_BASE = "/api/analytics";

const charts = {
  course: null,
  group: null,
  learner: null,
};

const elements = {
  form: document.querySelector("#filters-form"),
  courseTable: document.querySelector("#course-table"),
  groupTable: document.querySelector("#group-table"),
  learnerTable: document.querySelector("#learner-table"),
  courseDownload: document.querySelector("#download-course"),
  groupDownload: document.querySelector("#download-group"),
  learnerDownload: document.querySelector("#download-learner"),
};

document.addEventListener("DOMContentLoaded", () => {
  if (!elements.form) {
    console.error("Admin analytics form not found");
    return;
  }

  elements.form.addEventListener("submit", (event) => {
    event.preventDefault();
    refreshDashboard();
  });

  elements.form.addEventListener("reset", () => {
    window.setTimeout(refreshDashboard, 50);
  });

  refreshDashboard();
});

function readFilters() {
  const courseId = Number.parseInt(document.querySelector("#filter-course").value, 10);
  const groupId = Number.parseInt(document.querySelector("#filter-group").value, 10);
  const learnerId = Number.parseInt(document.querySelector("#filter-learner").value, 10);
  const status = document.querySelector("#filter-status").value || undefined;
  const startDate = document.querySelector("#filter-start").value || undefined;
  const endDate = document.querySelector("#filter-end").value || undefined;

  return {
    course_id: Number.isNaN(courseId) ? undefined : courseId,
    group_id: Number.isNaN(groupId) ? undefined : groupId,
    learner_id: Number.isNaN(learnerId) ? undefined : learnerId,
    status,
    start_date: startDate,
    end_date: endDate,
  };
}

async function fetchJSON(endpoint, params) {
  const searchParams = new URLSearchParams();
  Object.entries(params)
    .filter(([, value]) => value !== undefined && value !== null && value !== "")
    .forEach(([key, value]) => searchParams.append(key, value));

  const query = searchParams.toString();
  const url = query ? `${API_BASE}/${endpoint}?${query}` : `${API_BASE}/${endpoint}`;
  const response = await fetch(url);
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to fetch analytics data");
  }
  return response.json();
}

function toCsvLink(endpoint, params) {
  const searchParams = new URLSearchParams();
  Object.entries(params)
    .filter(([, value]) => value !== undefined && value !== null && value !== "")
    .forEach(([key, value]) => searchParams.append(key, value));
  const query = searchParams.toString();
  return query ? `${API_BASE}/${endpoint}.csv?${query}` : `${API_BASE}/${endpoint}.csv`;
}

function refreshDownloadLinks(filters) {
  elements.courseDownload.href = toCsvLink("course-progress", filters);
  elements.groupDownload.href = toCsvLink("group-progress", filters);
  elements.learnerDownload.href = toCsvLink("learner-progress", filters);
}

function renderTable(target, rows) {
  if (!rows || rows.length === 0) {
    target.innerHTML = "<thead><tr><th>No data</th></tr></thead>";
    return;
  }

  const headers = Object.keys(rows[0]);
  const thead = `<thead><tr>${headers.map((header) => `<th>${header.replaceAll("_", " ")}</th>`).join("")}</tr></thead>`;
  const tbody = `<tbody>${rows
    .map(
      (row) =>
        `<tr>${headers
          .map((header) => `<td>${formatValue(row[header])}</td>`)
          .join("")}</tr>`
    )
    .join("")}</tbody>`;
  target.innerHTML = `${thead}${tbody}`;
}

function formatValue(value) {
  if (value === null || value === undefined) {
    return "—";
  }
  if (typeof value === "number") {
    return Number.isInteger(value) ? value : value.toFixed(1);
  }
  if (typeof value === "string" && value.endsWith("Z")) {
    const date = new Date(value);
    if (!Number.isNaN(date.valueOf())) {
      return date.toLocaleString();
    }
  }
  return value;
}

function renderChart(key, canvasId, labels, dataset) {
  const ctx = document.getElementById(canvasId);
  const data = {
    labels,
    datasets: [
      {
        label: "Average Progress (%)",
        data: dataset,
        backgroundColor: "rgba(99, 102, 241, 0.5)",
        borderColor: "rgb(99, 102, 241)",
        borderWidth: 1,
      },
    ],
  };

  if (charts[key]) {
    charts[key].data = data;
    charts[key].update();
    return;
  }

  charts[key] = new window.Chart(ctx, {
    type: "bar",
    data,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          beginAtZero: true,
          max: 100,
          ticks: {
            callback: (value) => `${value}%`,
          },
        },
      },
    },
  });
}

function renderLearnerChart(rows) {
  const ctx = document.getElementById("learner-chart");
  const dataPoints = rows.map((row) => ({
    x: row.progress_percent || 0,
    y: row.score || 0,
    r: Math.max((row.completed_activities || 0) * 2, 6),
    label: `${row.first_name} ${row.last_name}`,
  }));

  const data = {
    datasets: [
      {
        label: "Learner progress vs score",
        data: dataPoints,
        backgroundColor: "rgba(34, 197, 94, 0.6)",
        borderColor: "rgba(34, 197, 94, 0.9)",
      },
    ],
  };

  if (charts.learner) {
    charts.learner.data = data;
    charts.learner.update();
    return;
  }

  charts.learner = new window.Chart(ctx, {
    type: "bubble",
    data,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        x: {
          beginAtZero: true,
          max: 100,
          title: {
            display: true,
            text: "Progress (%)",
          },
        },
        y: {
          beginAtZero: true,
          max: 100,
          title: {
            display: true,
            text: "Score",
          },
        },
      },
      plugins: {
        tooltip: {
          callbacks: {
            label(context) {
              const point = context.raw;
              return `${point.label}: progress ${point.x}%, score ${point.y}`;
            },
          },
        },
      },
    },
  });
}

async function refreshDashboard() {
  const filters = readFilters();
  refreshDownloadLinks(filters);

  try {
    const [courseData, groupData, learnerData] = await Promise.all([
      fetchJSON("course-progress", filters),
      fetchJSON("group-progress", filters),
      fetchJSON("learner-progress", filters),
    ]);

    renderTable(elements.courseTable, courseData);
    renderTable(elements.groupTable, groupData);
    renderTable(elements.learnerTable, learnerData);

    renderChart(
      "course",
      "course-chart",
      courseData.map((row) => row.course_title || `Course ${row.course_id}`),
      courseData.map((row) => row.average_progress_percent || 0)
    );

    renderChart(
      "group",
      "group-chart",
      groupData.map((row) => row.group_name || `Group ${row.group_id}`),
      groupData.map((row) => row.average_progress_percent || 0)
    );

    renderLearnerChart(learnerData);
  } catch (error) {
    console.error(error);
    alert(`Unable to refresh analytics: ${error.message}`);
  }
}
