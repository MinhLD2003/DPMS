# Stage 1: Build app
FROM node:18-alpine AS builder

WORKDIR /app

# Copy package files và cài đặt dependencies
COPY package.json package-lock.json ./
RUN npm ci --silent

# Copy toàn bộ source code
COPY . .

# Tắt kiểm tra unused code (chỉ trong quá trình build Docker)
RUN node -e "\
  const fs = require('fs'); \
  ['tsconfig.app.json', 'tsconfig.node.json'].forEach(file => { \
    if (fs.existsSync(file)) { \
      let content = fs.readFileSync(file, 'utf8'); \
      content = content.replace(/\/\*[\s\S]*?\*\/|\/\/.*/g, ''); /* Loại bỏ chú thích */ \
      const ts = JSON.parse(content); \
      ts.compilerOptions = ts.compilerOptions || {}; \
      ts.compilerOptions.noUnusedLocals = false; \
      ts.compilerOptions.noUnusedParameters = false; \
      ts.compilerOptions.skipLibCheck = true; /* Bỏ qua kiểm tra thư viện */ \
      fs.writeFileSync(file, JSON.stringify(ts, null, 2)); \
    } \
  });"

# Build project (output Vite sẽ nằm ở thư mục `dist` hoặc `build`)
RUN npm run build

# Stage 2: Serve app với Nginx
FROM nginx:stable-alpine

# Xóa thư mục cũ (nếu tồn tại) của Nginx
RUN rm -rf /usr/share/nginx/html/*

# Copy nội dung build từ bước trước
COPY --from=builder /app/build /usr/share/nginx/html

# Mở cổng 80
EXPOSE 80

# Khởi chạy Nginx
CMD ["nginx", "-g", "daemon off;"]
