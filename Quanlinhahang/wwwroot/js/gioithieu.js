document.addEventListener("DOMContentLoaded", function () {
  const images = document.querySelectorAll(".gallery-item img");
  const lightbox = document.querySelector(".lightbox");
  const lightboxImg = document.querySelector(".lightbox-content");
  const closeBtn = document.querySelector(".close");

  // Khi click vào ảnh -> phóng to
  images.forEach((img) => {
    img.addEventListener("click", () => {
      lightbox.style.display = "flex";
      lightboxImg.src = img.src;
    });
  });

  // Click nút đóng
  closeBtn.addEventListener("click", () => {
    lightbox.style.display = "none";
  });

  // Click ra ngoài ảnh cũng tắt
  lightbox.addEventListener("click", (e) => {
    if (e.target === lightbox) {
      lightbox.style.display = "none";
    }
  });
});
document.addEventListener("DOMContentLoaded", function () {
  // Tải file navbar.html
  fetch("../navbar/navbarfinal.html") // <-- CHỈNH LẠI ĐƯỜNG DẪN NÀY
    .then((response) => {
      if (!response.ok) {
        throw new Error("Không thể tải navbar.html");
      }
      return response.text();
    })
    .then((data) => {
      // Gắn nội dung navbar vào div có id="navbar-container"
      document.getElementById("navbar-container").innerHTML = data;

      // Sau khi chèn navbar, nạp thêm CSS & JS của navbar nếu cần
      const link = document.createElement("link");
      link.rel = "stylesheet";
      link.href = "../navbar/navbarfinalcss.css";
      document.head.appendChild(link);

      const script = document.createElement("script");
      script.src = "../navbar/navbarfinaljs.js";
      document.body.appendChild(script);
    })
    .catch((error) => console.error("Lỗi khi tải navbar:", error));
});
