name: Bug Report
description: Create a report to help us fix a bug or issue.
title: "[BUG] "
labels: ["bug"]
body:
  - type: textarea
    id: description
    attributes:
      label: Bug Description
      description: A clear description of the bug.
    validations:
      required: true
